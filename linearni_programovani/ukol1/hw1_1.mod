param n, integer, > 0;
set Vertices := 0..n-1;
set Edges within Vertices cross Vertices;
var x{v in Vertices}, >=0;
var max, >=0;

minimize obj: max;
cond_edge{(i,j) in Edges}: x[j] - x[i] >=1;
cond_max{i in Vertices}: max - x[i] >= 0;

solve;
printf "#OUTPUT: %d\n", obj;
printf{i in Vertices} "v_%d: %d\n", i, x[i];
printf "#OUTPUT END\n";
end;
