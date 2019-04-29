param n, integer, > 0;
set V := 0..n-1;
set E within V cross V;
param w{(i,j) in E}, >= 0, integer;
var x{(i,j) in E}, >= 0;

minimize obj: sum{(i,j) in E} w[i,j] * x[i,j];
tri{(i,j) in E, (j,k) in E}: if (k,i) in E then x[i,j] + x[j,k] + x[k,i] else 1 >= 1;
quad{(i,j) in E, (j,k) in E, (k,l) in E}: if (l,i) in E then x[i,j] + x[j,k] + x[k,l] + x[l,i] else 1 >= 1;

solve;
printf "#OUTPUT: %d\n", obj;
for {(i,j) in E: x[i,j] > 0} 
{
    printf "%d --> %d\n", i, j, w[i,j];
}
printf "#OUTPUT END\n";
end;
