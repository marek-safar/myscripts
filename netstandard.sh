#!/bin/sh

if [ "x$1" = 'x-clean' ]; then
	rm -rf dlls packages ref report* nuget.exe
fi

MONODIR=/Users/marek/git/mono/
#MONO_ASM_DIR=/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.5
MONO_ASM_DIR=$MONODIR/mcs/class/lib/monodroid

# need at least NuGet 3, Mono 4.1 ships 2.8.4
if [ ! -e nuget.exe ]; then
	curl https://dist.nuget.org/win-x86-commandline/latest/nuget.exe >nuget.exe || exit 1
fi

# for some reason Mono doesn't even build mono-api-html or mono-api-diff
# but ships mono-api-info
APIDIFF_PROJ=$MONODIR/mcs/tools/mono-api-html/mono-api-html.csproj
if [ ! -e $APIDIFF_PROJ ]; then
	echo "need $MONODIR checked out"
	exit 1
fi
xbuild $MONODIR/mcs/tools/mono-api-html/mono-api-html.csproj || exit 1
APIDIFF=$MONODIR/mcs/tools/mono-api-html/bin/Debug/mono-api-html.exe

# we need a patched mono-api-info anyway
xbuild $MONODIR/mcs/tools/corcompare/mono-api-info.csproj || exit 1
APIINFO=$MONODIR/mcs/tools/corcompare/bin/Debug/mono-api-info.exe

#REFS=/Users/marek/git/mono/mcs/netstandard/refs
REFS=/Users/marek/git/mono/mcs/netstandard/dlls

mkdir -p ref

# copy all the reference dlls to a single directory so
# cecil can resolve references
#mkdir -p dlls
#for VERSION in "1.0" "1.1" "1.2" "1.3" "1.4" "1.5" "1.6"; do  
#	for DLL in `find ./packages | grep "ref/netstandard$VERSION/[^/]*.dll"`; do
#		cp $DLL dlls
#	done
#done

#for DLL in dlls/*; do
#	 mono $APIINFO -f $DLL > ref/`basename $DLL .dll`.xml || exit 1
#done

## OLD for Eric code

#
#for DLL in $REFS/*; do
#	INFO_FILE="ref/`basename $DLL .dll`.xml"
#	echo "creating $INFO_FILE"
#	mono $APIINFO -f $DLL > $INFO_FILE || exit 1
#done

echo "<html>" > report.html
rm -f log.txt
mkdir -p info
for DLL in $REFS/*; do
	NAME=`basename $DLL .dll`
	DLL=$MONO_ASM_DIR/Facades/$NAME.dll
	XMLNAME=`basename $DLL .dll`.xml

	if [ ! -e $DLL ]; then
		DLL=$MONO_ASM_DIR/$NAME.dll
	fi

	if [ ! -e $DLL ]; then
		echo "creating dummy info/$XMLNAME"
		echo "<?xml version=\"1.0\" encoding=\"utf-8\"?>
<assemblies>
  <assembly name=\"$NAME\" version=\"4.0.1.0\">
    <attributes />
    <namespaces />
  </assembly>
</assemblies>" > info/$XMLNAME
	else
		echo "creating info/$XMLNAME"
		mono $APIINFO --contract-api -f -d  $MONO_ASM_DIR $DLL > info/$XMLNAME || exit 1
	fi

	rm -f report_$NAME.html

	mono $APIDIFF --ignore-nonbreaking ref/$XMLNAME info/$XMLNAME report_$NAME.html >> log.txt || exit 1

	if [ -e report_$NAME.html ]; then
		cat report_$NAME.html >> report.html
	else
		echo "<h1>$NAME.dll</h1><h2>Unchanged</h2>" >> report.html
	fi
done
echo "</html>" >> report.html
