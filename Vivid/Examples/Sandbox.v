init() {
	println('Hello guture!')
}

A {
	x: large

	h(d: large): decimal
}

goo() {
	=> A()
}

export foo(a: large, b: large, r: large)
{
	=> a - (a / b) * b
}

export assign_add_2(a: A) {
	a.x += 2000000
}

export assign_subtract_2(a: A) {
	a.x -= 2
}

export assign_multiply_2(a: A) {
	a.x *= 2
}

export assign_divide_2(a: A) {
	a.x /= 2
}

export assign_multiply_32(a: A) {
	a.x *= 32
}

export assign_divide_32(a: A) {
	a.x /= 32
}

export assign_multiply_32(a: A, b: large) {
	a.x *= b
}

export assign_divide_32(a: A, b: large) {
	a.x /= b
}

export assign_remainder_32(x: large) {
	=> x % 3
}

export a(x: large, y: large) {
	=> x <| y
}

export b(x: large, y: large) {
	=> x |> y
}

export c(x: large, y: large) {
	=> -x
}

export d(x: large, y: large) {
	=> x & y
}

export e(x: large, y: large) {
	=> x | y
}

export f(x: large, y: large) {
	=> x ¤ y
}

export g(a: A, y: large) {
	a.x &= y
}

export h(a: A, y: large) {
	a.x |= y
}

export i(a: A, y: large) {
	a.x ¤= y
}

export gg(a: A) {
	=> a.h(0.0) as large
}

export qq(a: A, x: large, y: large) {
	=> x % y
}

export m3(x: large) => x * 4097

export ma3(a: A) {
	a.x *= 4097
}