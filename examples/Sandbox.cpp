#include <sys/mman.h>
#include <system_error>

int sum(int a, char c, short b)
{
    return a + b + c;
}

int main() 
{
    char c = 7;
    return sum(3, 5, c);
}