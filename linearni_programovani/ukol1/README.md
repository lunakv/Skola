## Installation
Extract the archive to your installation directory and run  
```
make
```
A `lunakv_1_x` executable file will be generated for each homework.

By default, the programs clean up all files they generate. If you want to preserve these files, build the programs using 
```
make debug
```
instead. If built this way, the programs will create a `hw1_x.dat` file in the installation directory whenever they're invoked.


## Usage
The program reads input from `STDIN` and produces output on `STDOUT`. Running the program is as simple as invoking the command and providing the input stream, e.g.
```
./lunakv_1_1 < vstupy/vstup1-xxx.txt
```
## Output
Assuming the input data is in the correct format, the program outputs the result of the computation. Apart from some `glpsol` processing information, the solution, if it exists, will be shown between the `#OUTPUT: n` line and the `#OUTPUT END` line, where `n` is the solution to the LP. Between these lines is written the following:
* For the fist program, a line for each vertex displaying 
```
v_X: N
```
where `X` is the vertex number and `N` is its position in the ordering.
* For the second program, a line for each edge contained in the cover displaying
```
I --> J
```
for a line going from `I` to `J`.

If the input is in the correct format, but a solution cannot be found, none of these lines are displayed and instead the output contains the following line:
```
LP HAS NO PRIMAL FEASIBLE SOLUTION
```

If the input is in an incorrect format, none of these lines are displayed and instead the output is a single line starting with 
```
Error:
```
describing the error.

## No solution
If the first program has no feasible solution, it means the vertices cannot be ordered, meaning at least one directed cycle exists in the graph.

The second program must always have a solution, since removing all edges is a feasible solution for any graph. 