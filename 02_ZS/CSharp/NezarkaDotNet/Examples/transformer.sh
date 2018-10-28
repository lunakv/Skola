cat "$1" | awk '
{
    printf "Console.Writeline(\"%s\");", $0
    print ""
}    
    
    '