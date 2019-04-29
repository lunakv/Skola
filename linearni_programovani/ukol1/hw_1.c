#include <stdio.h>
#include <stdlib.h>
#include <libgen.h>
#include <string.h>
#include <unistd.h>

void cleanup(void);

int main() {
    // DIR = the installation directory
    #ifndef DIR
    puts("Compilation error: DIR needs to be specified.");
    return EXIT_FAILURE;
    #else

    chdir(DIR);

    int vert;
    int edge;
    if (scanf("DIGRAPH %d %d:", &vert, &edge)) {
        FILE* stream = fopen("hw1_1.dat", "w+");
        fprintf(stream, "data;\nparam n := %d;\n", vert);
        fprintf(stream, "set Edges := \n");
        
        int from = 0;
        int to = 0;
        // Reading the input and generating the Edge set
        for (size_t i = 0; i < edge; i++)
        {
            if (scanf("%d --> %d", &from, &to))
            {
                fprintf(stream, "%d %d\n", from, to);
            } else {
                fclose(stream);
                puts("Error: Invalid line specification format.");
                cleanup();
                return EXIT_FAILURE;
            }
        }
        fprintf(stream, ";\nend;\n");
        fclose(stream);

        // Finding the solution
        char* command = "glpsol -m hw1_1.mod -d hw1_1.dat";
        system(command);
        
        cleanup();
    } else {
        puts("Error: Invalid header line format.");
        return EXIT_FAILURE;
    }

    #endif
}

void cleanup(void) {
    #ifndef DEBUG
    remove("hw1_1.dat");
    #endif
}