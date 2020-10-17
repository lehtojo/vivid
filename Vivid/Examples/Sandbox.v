import large_function()

f(x, y, z, w) {
    => x + y - z * w
}

g<T>(a: T, b: T) {
    => a + b
}

h<X, Y, Z>(a: X, b: Y, c: Z) {
    => a + b + c
}

outline template_function_calls() {
    g<num>(1, 2)
    h<tiny, decimal, large>(1 as tiny, 2.0 as decimal, 3)
}

Banana<X, Y> {
    a: X
    b: Y
}

get_value<X, Y>(value: X) {
    => value.b.a as Y
}

outline template_types() {
    banana = Banana<small, normal>()
    banana.a = 1
    banana.b = 2
}

outline drift(n) {
    loop (i = 0, i < n, i++) {
        n++
    }
}

outline shorts(a) {
    if a > 0 a++
    else a < 0 a--

    => a
}

outline decimals() {
    printsln(to_string_decimal(3.14159))
}

outline d(a, b, c) {
    => a - (b > c) as num
}

q1(i) => ++i + i++

q2(i) => --i + i--

q3(i) => i++ + ++i

q4(i) => i-- + --i

q5(i: tiny, j: normal) => i + j

pepepopo(a: decimal, b: decimal, n: num) {
    # Single line comment
    loop (i = 0, i < n, i++) {
        large_function()
    }

    => a + b
}

outline pewpew(a) => (a > 10)

init() {
    printsln(to_string_decimal(-3.14159))

    q1(1)
    q2(1)
    q3(1)
    q4(1)
    q5(1, 1)
    shorts(1)

    if !pewpew(10) {
        printsln(to_string_decimal(-2.14159))
    }
}

###
init() {
    template_function_calls()
    template_types()

    super = Banana<Banana<decimal, decimal>, Banana<num, num>>()
    get_value<Banana<Banana<decimal, decimal>, Banana<num, num>>, Banana<num, num>>(super)

    drift(10)
    shorts(-10)
    decimals()
    d(1, 2, 3)
    
    q1(10)
    q2(10)
    q3(10)
    q4(10)

    pepepopo(1.0, 1.0, 8)

    => f(1, 2, 3, 4)
}
###