# Tests whether the two integer statements will be converted into decimal return statements
export automatic_number_conversion(a: large) {
	if a > 0
		=> 2 * a
	else a < 0
		=> a
	else
		=> 1.0
}

# Tests whether integer to decimal cast works
export casts_1(a: large) {
	=> a as decimal
}

# Tests whether decimal to integer cast works
export casts_2(a: decimal) {
	=> a as large
}

# Tests boolean cast
export casts_3(a: large) {
	=> a as bool
}

Foo {
	a: tiny
	b: small

	init(a, b) {
		this.a = a
		this.b = b
	}
}

Bar {
	c: normal
	d: large

	init(c, d) {
		this.c = c
		this.d = d
	}

	virtual bar(): decimal
}

Foo Bar Baz {
	e: decimal

	init(x: decimal) {
		Foo.init(x, x + 1)
		Bar.init(x + 2, x + 3)
		
		e = x + 4
	}

	override bar() {
		if a + b == c + d {
			=> a + b
		}

		=> c + d
	}
}

# Creates an instance of Baz and returns it
export create_baz() {
	=> Baz(0.0)
}

# Creates an instance of the type Baz and tests whether the inner assignment statements work since they require conversions from the input type decimal
export casts_4(x: decimal) {
	=> Baz(x)
}

# Tests whether base class casts work
export casts_5(baz: Baz) {
	=> baz as Foo
}

# Tests whether base class casts work
# NOTE: Here the base class is actually in the middle of the allocated memory so the compiler must add some offset to the pointer
export casts_6(baz: Baz) {
	=> baz as Bar
}

# Tests whether the compiler automatically casts the Baz object into a Bar object since it will be the return type
export automatic_cast_1(baz: Baz) {
	if baz.e >= 1.0 => baz

	=> Bar(baz.e, baz.e)
}

# Tests whether the return statements in the implementation of the function bar will obey the declared return type decimal.
# In addition, the compiler must cast the self pointer to type Bar.
export automatic_cast_2(baz: Baz) {
	=> baz.bar()
}

# Tests whether the loaded tiny from the specified address will be converted into a large integer
export automatic_conversion_1(a: link) {
	if a => a[0]

	=> 0
}

# Tests whether the loaded small from the specified address will be converted into a large integer
export automatic_conversion_2(a: link<small>) {
	if a => a[0]

	=> 0
}

# Tests whether the loaded normal from the specified address will be converted into a large integer
export automatic_conversion_3(a: link<normal>) {
	if a => a[0]

	=> 0
}

# Tests whether the loaded large from the specified address will be converted into a large integer
export automatic_conversion_4(a: link<large>) {
	if a => a[0]

	=> 0
}

# Tests whether the loaded decimal from the specified address will be converted into a large integer
export automatic_conversion_5(a: link<decimal>) {
	if a => a[0] as large

	=> 0
}

B {
	x: large
	y: small
	z: decimal
}

A {
	b: B
}

export assign_addition_1(a: large, b: large, i: A, j: large) {
	a += b
	i.b.x += j
	i.b.y += j
	i.b.z += j
	=> a
}

export assign_subtraction_1(a: large, b: large, i: A, j: large) {
	a -= b
	i.b.x -= j
	i.b.y -= j
	i.b.z -= j
	=> a
}

export assign_multiplication_1(a: large, b: large, i: A, j: large) {
	a *= b
	i.b.x *= j
	i.b.y *= j
	i.b.z *= j
	=> a
}

export assign_division_1(a: large, b: large, i: A, j: large) {
	a /= b
	i.b.x /= j
	i.b.y /= j
	i.b.z /= j
	=> a
}

export assign_remainder_1(a: large, b: large, i: A, j: large) {
	a %= b
	i.b.x %= j
	i.b.y %= j
	# Remainder operation is not defined for decimal values
	=> a
}

export assign_bitwise_and_1(a: large, b: large, i: A, j: large) {
	a &= b
	i.b.x &= j
	i.b.y &= j
	# Bitwise operations are not defined for decimal values
	=> a
}

export assign_bitwise_or_1(a: large, b: large, i: A, j: large) {
	a |= b
	i.b.x |= j
	i.b.y |= j
	# Bitwise operations are not defined for decimal values
	=> a
}

export assign_bitwise_xor_1(a: large, b: large, i: A, j: large) {
	a ¤= b
	i.b.x ¤= j
	i.b.y ¤= j
	# Bitwise operations are not defined for decimal values
	=> a
}

export assign_multiplication_2(a: large, b: large, c: large, d: large, i: A, j: A, k: A, l: A) {
	a *= 2
	b *= 5
	c *= 51
	d *= -8

	i.b.x *= 2
	i.b.y *= 2
	i.b.z *= 2

	j.b.x *= 5
	j.b.y *= 5
	j.b.z *= 5

	k.b.x *= 51
	k.b.y *= 51
	k.b.z *= 51

	l.b.x *= -8
	l.b.y *= -8
	l.b.z *= -8

	=> a * b * c * d
}

export assign_division_2(a: large, b: large, c: large, d: large, i: A, j: A, k: A, l: A) {
	a /= 2
	b /= 5
	c /= 51
	d /= -8

	i.b.x /= 2
	i.b.y /= 2
	i.b.z /= 2

	j.b.x /= 5
	j.b.y /= 5
	j.b.z /= 5

	k.b.x /= 51
	k.b.y /= 51
	k.b.z /= 51

	l.b.x /= -8
	l.b.y /= -8
	l.b.z /= -8

	=> a * b * c * d
}

init() {
	casts_4(1.0)
}