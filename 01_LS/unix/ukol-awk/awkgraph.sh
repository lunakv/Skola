#!/bin/sh

awk '
BEGIN {FS="[),(]"}
{
        if( NR == 1 && $1 != "graph")
        {
                print "First line must initialize the graph. Use *graph(n)* " > "/dev/stderr"
                exit 1
        }
        else if (NR == 1)
        {
                n = $2;
                FS = " "
                next
        }
        else if (NR <= n+1)
        {
                for (i = 1; i <= n; i++)
                if ($i != 0 && $i != 1)
                {
                	print "Invalid matrix format detected on line", NR > "/dev/stderr/"
                    exit 1
                }
                else
                    matice[NR-1][i] = $i
                
                if (NR == n+1)
                    FS = "[),(]"
        }
        else if ($1 == "exist")
        {
                print matice[$2][$3]
        }
        else if ($1 == "add")
        {
                matice[$2][$3] = 1
                matice[$3][$2] = 1
        }
        else if ($1 == "delete")
        {
                matice[$2][$3] = 0
                matice[$3][$2] = 0
                next
        }
        else if ($1 == "neighbours")
        {
                printcomma = 0
                for (i = 1; i <= n; i++)
                if (matice[$2][i] == 1)
                {
                        if (printcomma == 1)
                                printf ","
                        else
                                printcomma = 1

                        printf i ;
                }
                print ""
        }
        else if ($1 == "degree")
        {
                Deg = 0;
                for (i = 1; i <= n; i++)
                if (matice[$2][i] == 1)
                        Deg++;
                print Deg
        }
        else if ($1 == "isolate")
        {
                for (i = 1; i <= n; i++)
                {
                        matice[$2][i] = 0
                        matice[i][$2] = 0
                }
        }
        else
        {
                print "Invalid command: ", $0 > "/dev/stderr"
        }
}
END {
        for (i = 1; i <= n; i++)
        {
                for (j = 1; j <= n; j++)
                        printf "%s ",matice[i][j];
                print ""
        }
}
'