#include <sys/mman.h>
#include <system_error>

char sum(char a, char b)
{
    return a + b;
}

int main() 
{
    return sum(3, 5);
}