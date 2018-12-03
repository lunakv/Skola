cat "$1" | sed -e 's/"/\\"/g' | awk '
{
    
    printf "Console.Writeline(\"%s\");", $0
    print ""
}    
    
    '
