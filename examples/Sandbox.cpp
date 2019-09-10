#include <iostream>
#include <vector>
#include <ctime>
#include <sys/mman.h>
using namespace std;

extern "C" int a(const char* text);
extern "C" int b(const char* text);

const char* text = "tZm8JbjRf79PU0LQl8wyECaH8qXB7lcowMKeKLJdFBN8W7O3Fki2B1uTNFUc6kiSXyCPq40siUDcb8FLBmTKUigOb8ZtiGzoYInSRpYKYNyLyBS0IxjZXwWRdylsbU607gq80xukw5FoP1ErxDPWUBtPr0uFhgvoNaHUN80e9e0YuO1CEWAwYqBZHpUmi1eZKvmRFGTVQzp2w85abh0JOPAR9XgwhBcs5hyTDLV30sH4ZLECb5XPkc3 Tqd7Vw1QRYKYXhjLBUTJUlTpiKDnrR66LqgwTLQ4dm5j1qLanzDPlaJP ea9TZzVHBy0RwW1pxLQDGScTMfe2ecEtLMdiW4S2PWI506I5mo9EoOkP6aheib3dX4gZ1bq0uhPwzXUqikH27b0byjlWsAUqeUYVLqP8XF3ncumYKrw9bHp2EvA4x0X37DdLVGdpz7nmaCoV kMQwjdymyIwRBd7pGmCxNJuq3V2fScWz5DOb84ROhg3pxgmI43kQy j75IMnIh9saTE86ZfeedH6iiMCWBgo17AONbokLy3 8sobuW8 stDMy1mt ajCI4oX2LpX14pvxduDt3Bw6h4Zt3jtFzNeuxylF2MkW6N8cvth9ehFCkRtWr0RKOZ9mRhMolFdVubO9fblnIZZLfFjtrh8uozGJJomYJ07bW cCO L4 8bFWWAAPzlzegHOkdcNuFX7rHqbt1WGZCzyfIxLQ5khr2iA86EzcRqJeG GaNcq2cyliwC9eT OmKwYfkPzamTFe5YUfaO KPH9EufRBVELmsUUzdJLzGPnPggRVaHCoLsV7nAtPYN2LrzBqyVwT6TFHNihpB3ijdPmBKyaXu1RJgi8ual2AHioeQKXebgwxra637fmVWcIOr5s8RZAqbVqpdMAXSAQX6xcuz b1v9F4GCZHUJPrDrL O5yK3hOYmMhOSkoLGIfByVvCWybJ6NbqONqIhWLY2aSjlI8RkfTVgbXkm73mtXe9jAbwDA0xkA2hzMVPg720eHl38eDriB XqxjUuVOluB48fj4lnUGGYkc1gNDEM3LtmaTNerW1QGK7dtVgcm 79mHKufyUIolHgYf9Kd41cCfj2YaA GOnz6jcZQNUf8ziIU0tUR5DvDrGQNKPpQNCMsM62qaKnCDbGqEF3xbZdT3n33sOPVVV7yLRH xU9seehT9b6n5nlexbJXCikyxhOo yDOuBe044NLiQA4TIz963jy7Zm8O9ca1XfF6yrA2v6AOX9vAodKzz28kO4B04hJbwMoc63rTmLP7WE6dQJ6j64KiNTtd2s4KkGRCiVGMtrwSvTofrYHHptRsrQT9FWMRZ 42b688lkDCiAcq6Yz mbapVH0nmhKu9lEd8Zv8naY2WhBNqqLHnwsSiDgvCqUFZC1Q1VHIiXhTkvDKTXgBxu6V1WIvr9YrtaBHZM57GQ2OgXEahechwWPKzupDck60N6UeSoa0RkaU1asaBHUocU3qslVSBBUDAiujDAiHAPpR5M0m0AhjxrAI9g0l6XA8CceZpBb LIyMBADvVKerP J3pS8nY7Hd9Z5KRvN1S6TWRIUf0TH2Tv9UjbLsqEVp5rMZtD3fUK1Svv5zb9n0mXoacr0XngVJ2BHxBD4IU6XD2J4uLFOsCHh ACzAl6MvXSaoDYDQ6VLItmkcu0DXvI7EFiNLJodN6RcHXobhjAYxQBAkEpAIUmLPJStH23hniDhDgGRLOanps6tU60VvpCPXNNBPhdaNLu243i6DjqZobk51DwaQ8pmkXXJkZtIOy6Jmz9fUVXx7qGOFKzmyaFz eodyXezhiyDtRp2L0s64Dt4wSKDj r9DnQI5 JEPpxBsChGnOA2hVrRB6siEOx Iq5uB1kATU1594Nuv5qXD7MsMIYNUjQcPkLREsEanKjhEGECW6D9ya7WiAKWxIoLJTecTP4riuqLB yhY6EwKVLuKjl02Zv80HKkeWD7FtAbLepeY9ZanULOPnqulUVfZncgNh4f4d9nVKWKZJY4w4ovJhclO1omC9dQcPkkiD6pdGywfFROnvEr59yv1EzXJi9UICrVl0f1uhqpC LC3TsM5OZU1tsedTowgNCqP3PygGMEnOj97yPETBoeuITN1WoePlUkLydY5cEnoQKgjFjyA2eCkEBjal9OEhlQjTOx6VtRagsDbY2lHr 6oihJNdvOqVr A9fDHDZ1B4u9gbQ6iJC1RkkoOOfEbqkRcJ66fnHOJYpcQh3vubnUoO1PzVsxEPiGNKPoEtJX79jMycxjRWF8xcyU998Ex e XAoZNj02fuOZ3io AU7vTPz1HuDnBkQ 8r1BQn1bAtpusdtUVDuOouL4w9ILl1BoOFfZhlcPAnevh92mT4FxYyZ54jvoULJqL RkTTDRkCA5RT0wfqW y8D5cC mUxRN29AOx37jU9j7QKp9 JxqMuI3H GdFXHO3Vl3DuqfikyzPp0lsCzupbFWwrK6aZ1orVcwGjzX0sf1s4C1ad0o3sT9vL30CBJq4gNp9x3LWy0SkiA7kAjf9LvdkaWKq6wyTb8w9G7aqwRJwGuWqpO5bjlancFY687on7lzRsahc2zFVlAJtezeJ8bqk5YuhFnE9M9ItA1 j9gv6HGoAZsntPHx5H5xLnsKFcQQPW121SyjaZLJyLvKu ofZkELvWSbzqa62sNKU6wWpT6KKuZvKb7jwW6l6eHXxx3Hlj9V6wFHD6mJ7NvL5nfz750ErEIK4pYiQzXaYGNjGoJ7S z1tKDzYoqqeFISuMt5fEAiDrSVt9OkZVlwrE9iRqtJyiubU3jR4zfrff9R6leKWzFgv 0dBxf8j0uOlKO2cpo0yzFO1syZ4eMp52kabYEDeqkInOUUVlOiMIvQ LQ3F9VYMcxQlfq5Z7bByLc4le4n4doGZ03xi7S5R96fzNE6KOIPC5hCatCn0VNbk8tTa KCPWqyQ1zG Z1hpUMZ6aPThgeXXnZ NSuw9uAf0f2cNxyvl8OilayH7A6tH6Ou68nzTacA8LHjIVMbmm6QlMJIAuomr9cSBMNniSh6cNuhm9sWTr 2KsBlfkz2Vqdia0XG9x9NmC9fjwJwEvfZNbUt2F8kN2CqrJSU6zqe5yTSpWyfb9EqB03aFEgeKuio FzMEVrTwiIoo1DKeMbLgbzqHUHzakhlWmKVSxarOEr4ipTHq 3 FO35zzTQ5suy5s6 RBUrqyDlaOo2tuZaTRRWgD2rI zBwEYmoxPfl2MJavm2mGW70F8W7J wU03a9HacLCLTywHcU97JyebPM6j o3D4o2VOQVR0EAsNWsJ8i0ENlPbX5WaJYjt1kvMeHcej2woiZSED1nbo3NdbVJgYdLOx5HU3N59U4Rpu9WDnLV3zJkYrCuaPjxL9QVetLm1o8Vk2LYOz1AW3IJ0toCYaiLKnjr IHMn1UrcVxmDk8r9D9AqqwkXH4fwdct3E3HR1qzqhaNkbdqeBl5h4BQRtOEYXDXTSmfpGSgMmF7JTmXGH4ggPoY9zBCqSuuPhvLAwbQP5xjou5nwH02SrfzkE1QmIgJMrp3sjT7CLX1U8A9nH5xfEy5khrgmPuQmtsPLCQrHhq2hyELka07E8MFYBp1YlkxUf02EyaNjUQSy8ndvTzmQCoBRGLwBT2Yy67CWFlkkBdlrIaulgtYN2r92bbBIH0wuf5q4DHs4FYzrDUDp3lEPA2N54cibYgh8yO47m3GWfygW9tjBQXc9RHy4TeJOf f1cUstOZ7QM3xyoBs55phbHA0Xydf1hNfoRPlK88lRCwKwv7q8K5HIb7kpEex7QRfyrFJ5l75VPIlFhpBf5F52ZS2xkTRPBJRcE38gbyWSVQOxeVr7BUu1rRjAes6LlWEPyFnHh1oazJa4ras3xEauuLrQ0AsjYUMfkzQFUZHPeiTT80RJ IAE8g5xDKnHypcQe398K2mH0MCE9dw419n1NSUsh3gw3l0pK98008UFAiBEvmkvoh OMxQ3Vd9P9y7qhSWdXEYxlREzUCmHygYf9HJtwRxXpvwXuQ1iWeRwleKgEW4RaZsEzgmuR2SUyaZRKipohh2rAQEjm1caceETzMhjtNJ";

#define COUNT 10000

clock_t measure_one_a()
{
    clock_t start = clock();

    for (int i = 0; i < COUNT; i++)
    {
        a(text);
    }

    MAP_UNINI

    return clock() - start;
}

clock_t measure_one_b()
{
    clock_t start = clock();

    for (int i = 0; i < COUNT; i++)
    {
        b(text);
    }

    return clock() - start;
}

#define MEASUREMENTS 100

double measure_a()
{
    vector<clock_t> measurements;

    for (int i = 0; i < MEASUREMENTS; i++)
    {
        measurements.push_back(measure_one_a());
    }

    // Average
    clock_t sum = 0;

    for (clock_t time : measurements)
    {
        sum += time;
    }

    return sum / (double)measurements.size();
}

double measure_b()
{
    vector<clock_t> measurements;

    for (int i = 0; i < MEASUREMENTS; i++)
    {
        measurements.push_back(measure_one_b());
    }

    // Average
    clock_t sum = 0;

    for (clock_t time : measurements)
    {
        sum += time;
    }

    return sum / (double)measurements.size();
}

int main() 
{
    cout << a(text) << endl;
    return 0;
    
    // Fakes
    measure_a();
    measure_b();

    double a = measure_a();
    double b = measure_b();

    cout << "Scasb: " << a << endl;
    cout << "Loop: " << b << endl;

    if (a < b)
    {
        cout << "Scab was " << (b - a) / (double)a * 100.0 << "% faster";
    }
    else
    {
        cout << "Loop was " << (a - b) / (double)b * 100.0 << "% faster";
    }

    return 0;
}

// g++ -m32 -o Timer.o -c Sandbox.cpp
// yasm -f elf32 -o Functions.o Sandbox.asm
// g++ -m32 Timer.o Functions.o