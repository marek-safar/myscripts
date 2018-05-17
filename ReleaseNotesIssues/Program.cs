using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Octokit;
using System.Net.Http;

namespace ReleaseNotesIssues
{
	class Record
	{
		public enum Kind
		{
			GitHub,
			Bugzilla,
			Any
		}

		public Record (string id, double confidence, Kind kind, string original)
		{
			this.ID = id;
			this.confidence = confidence;
			this.kind = kind;
			this.original = original;
		}

		public string ID;
		double confidence;
		Kind kind;
		string original;

		static Regex bz_label = new Regex (@"<title>(\d+).*&ndash;(.*)<\/title>", RegexOptions.IgnoreCase);

		public string TryResolveDescription ()
		{
			switch (kind)
			{
			case Kind.Bugzilla:
				var title = IsValidBugzillaIssue (ID);
				if (title == null) {
					Console.WriteLine ($"Could not match BugZilla ID {ID}");
					return null;
				}

				return ProcessBugzillaIssue (ID, title);
			case Kind.GitHub:
				var valid = IsValidGithubIssue (ID);
				if (valid == null) {
					Console.WriteLine ($"Could not match GitHub ID {ID}");
					return null;
				}

				return ProcessGithubIssue (valid);

			case Kind.Any:
				var ghMaybe = IsValidGithubIssue (ID);
				if (ghMaybe != null)
					return ProcessGithubIssue (ghMaybe);

				var bzMaybe = IsValidBugzillaIssue (ID);
				if (bzMaybe != null)
					return ProcessBugzillaIssue (ID, bzMaybe); 

				Console.WriteLine ($"Could not match ID {ID}");
				return null;
			}

			return null;
		}

		static string ProcessGithubIssue (Issue issue)
		{
			if (issue.State != "closed") {
				if (!issue.Labels.Any (l => l.Name == "disabled-test" || l.Name == "flaky bug"))
					Console.WriteLine ($"WARNING: Issue {issue.Id} was reopened");

				return null;
			}

			return $"[#{issue.Number}](https://github.com/mono/mono/issues/{issue.Number}) - {issue.Title}";
		}

		static string ProcessBugzillaIssue (string id, string title)
		{
			return $"Bugzilla [{id}](https://bugzilla.xamarin.com/show_bug.cgi?id={id}) - {title}";
		}

		static Issue IsValidGithubIssue (string id)
		{
			return FindIssue (id);
		}

		static string IsValidBugzillaIssue (string id)
		{
			using (var http = new HttpClient ()) {
				try {
					var str = http.GetStringAsync ($"https://bugzilla.xamarin.com/show_bug.cgi?id={id}").Result;
					var match = bz_label.Match (str);
					if (match != null && match.Groups.Count == 3)
						return match.Groups [2].Value;

					return null;
				} catch (Exception e) {
					return null;
				}
			}
		}

		static Issue FindIssue (string id)
		{
			GitHubClient client = new GitHubClient (new Octokit.ProductHeaderValue ("Octokit.Samples"));
			client.Credentials = new Credentials ("------", "------");

			IIssuesClient issuesclient = client.Issue;
			try {
				var issue = issuesclient.Get ("mono", "mono", int.Parse (id)).Result;
				if (issue == null)
					return null;

				if (issue.PullRequest != null)
					return null;

				return issue;
			} catch (AggregateException e) {
				if (e.InnerException is NotFoundException)
					return null;

				throw;
			}
		}
	}

	class MainClass
	{
		public static void Main (string [] args)
		{
			// git log origin/2018-02..origin/2018-04 --abbrev-commit --no-merges
			const string file = @"/Users/marek/git/mono/mono/_changes.txt";

			var text = File.ReadAllText (file);

			List<Record> results = new List<Record> ();
			int pos = 0;
			int next;
			while ((next = text.IndexOf ("\ncommit ", pos + 8)) != -1) {
				var res = ProcessCommit (text.Substring (pos, next - pos));
				pos = next;

				if (res != null)
					results.Add (res);
			}

			var last = ProcessCommit (text.Substring (pos));
			if (last != null)
				results.Add (last);


			var urls = results.Select (l => l.TryResolveDescription ()).Where (l => l != null).Distinct ().ToList ();
			urls.Sort ();

			foreach (var url in urls)
				Console.WriteLine ("* " + url);
		}

		static Regex gh_issue = new Regex (@"mono/issues/(\d+)", RegexOptions.IgnoreCase);
		static Regex bz_issue = new Regex (@"bugzilla.xamarin.com/show_bug.cgi\?id=(\d+)", RegexOptions.IgnoreCase);

		static (Regex regex, Record.Kind kind)[] any_issue = new [] {
			(new Regex (@"fix for #?(\d+)", RegexOptions.IgnoreCase), Record.Kind.Any),
			(new Regex (@"fixes #?(\d+)", RegexOptions.IgnoreCase), Record.Kind.Any),
			(new Regex (@"bug #?(\d+)", RegexOptions.IgnoreCase), Record.Kind.Any),
			(new Regex (@"gh #?(\d+)", RegexOptions.IgnoreCase), Record.Kind.GitHub),
			(new Regex (@"bxc #?(\d+)", RegexOptions.IgnoreCase), Record.Kind.Bugzilla),
			(new Regex (@"GitHub #(\d+)", RegexOptions.IgnoreCase), Record.Kind.GitHub),
			(new Regex (@"for #(\d+)", RegexOptions.IgnoreCase), Record.Kind.GitHub),
		};

		static Record ProcessCommit (string text)
		{
			foreach (Match match in gh_issue.Matches (text)) {
				if (match.Groups.Count != 2)
					continue;

				var v = match.Groups [1].Value;
				if (v.Length < 3)
					continue;

				if (!int.TryParse (v, out _))
					throw new NotImplementedException ("this should not happen");

				return new Record (v, 1, Record.Kind.GitHub, text);
			}

			foreach (Match match in bz_issue.Matches (text)) {
				if (match.Groups.Count != 2)
					continue;

				var v = match.Groups [1].Value;
				if (v.Length < 3)
					continue;

				if (!int.TryParse (v, out _))
					throw new NotImplementedException ("this should not happen");

				return new Record (v, 1, Record.Kind.Bugzilla, text);
			}

			foreach (var any in any_issue) {
				foreach (Match match in any.regex.Matches (text)) {
					if (match.Groups.Count != 2)
						continue;

					var v = match.Groups [1].Value;
					if (v.Length < 3)
						continue;

					if (!int.TryParse (v, out _))
						throw new NotImplementedException ("this should not happen");

					return new Record (v, 1, any.kind, text);
				}
			}

//			if (text.IndexOf ("fixes", StringComparison.OrdinalIgnoreCase)> 0)
//				Console.Write (text);

			return default;
		}
	}
}
