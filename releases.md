Get list of contributors

git log origin/2018-02..origin/2018-04 --no-merges | grep ^Author: | sed 's/ <.*//; s/^Author: //' | sort -u | tr '\n' ","
