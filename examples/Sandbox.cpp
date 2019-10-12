#include <iostream>

extern "C"
{
    void function_hello()
    {
        system("notify-send Hellooo...");
    }
}