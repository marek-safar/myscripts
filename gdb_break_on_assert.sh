while true; do
MONO_PATH="./../../class/lib/build:$MONO_PATH" CSC_SDK_PATH_DISABLED= gdb -ex 'handle SIGXCPU SIG33 SIG35 SIGPWR nostop noprint'
-ex 'set args /home/marek/mono/external/roslyn-binaries/Microsoft.Net.Compilers/Microsoft.Net.Compilers.1.3.2/tools/csc.exe /code
page:65001 /nologo /noconfig /deterministic   -d:NET_4_0 -d:NET_4_5 -d:NET_4_6 -d:MONO  -nowarn:1699 -nostdlib -r:./../../class/l
ib/net_4_x/mscorlib.dll /debug:portable -optimize   /unsafe -resource:resources/SR.resources -resource:resources/SQLiteCommand.bm
p -resource:resources/SQLiteDataAdapter.bmp -resource:resources/SQLiteConnection.bmp -d:SQLITE_STANDARD -r:./../../class/lib/net_
4_x/System.dll -r:./../../class/lib/net_4_x/System.Data.dll -r:./../../class/lib/net_4_x/System.Transactions.dll -r:./../../class
/lib/net_4_x/System.Xml.dll   -target:library -out:../../class/lib/net_4_x/Mono.Data.Sqlite.dll  @Mono.Data.Sqlite.dll.sources' -
ex run /home/marek/mono/mono/mini/mono -ex quit
done
