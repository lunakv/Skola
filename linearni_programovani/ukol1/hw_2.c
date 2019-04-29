#include <stdio.h>
#include <stdlib.h>
#include <libgen.h>
#include <unistd.h>
#include <string.h>

typedef struct Edge Edge;
struct Edge{
    int from;
    int to;
    int weight;
};

int main(){
    // DIR = the installation directory
    #ifndef DIR
    puts("Compilation error: DIR needs to be specified.");
    #else 

    chdir(DIR);
    int vertC = 0;
    int edgeC = 0;

    if (scanf("WEIGHTED DIGRAPH %d %d:", &vertC, &edgeC)){
        Edge edges[edgeC];
        int from = 0;
        int to = 0;
        int weight = 0;

        // Loading the input into memory (must be written twice)
        for(size_t i = 0; i < edgeC; i++)
        {
            if (scanf("%d --> %d (%d)", &from, &to, &weight))
            {
                edges[i].from = from;
                edges[i].to = to;
                edges[i].weight = weight;
            } else {
                puts("Error: Invalid line specification format.");
                return EXIT_FAILURE;
            }
        }

        FILE *stream = fopen("hw1_2.dat", "w");
        fprintf(stream, "data; \nparam n := %d; \nset E := \n", vertC);

        // Creating the edge set
        for(size_t i = 0; i < edgeC; i++)
        {
            fprintf(stream, "%d %d\n", edges[i].from, edges[i].to);
        }
        fprintf(stream, ";\nparam w := \n");

        // Creating the weight parameters
        for(size_t i = 0; i < edgeC; i++)
        {
            fprintf(stream, "%d,%d %d\n", edges[i].from, edges[i].to, edges[i].weight);
        }
        fprintf(stream, ";\nend;\n");
        fclose(stream);

        // Finding the solution
        system("glpsol -m hw1_2.mod -d hw1_2.dat");
        
        // Clean up
        #ifndef DEBUG
        remove("hw_1_2.dat");
        #endif
    } else {
        puts("Error: Invalid input header format.");
        return EXIT_FAILURE;
    }

    #endif
}