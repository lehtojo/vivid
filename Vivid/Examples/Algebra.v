f(x) => 2 * x * x / x

g(x) {
	a = 1 + x
	b = 1 - x

	c = 3 * a + 5 * b + 7

	# 3 * (1 + x) + 5 * (1 - x) + 7 - (1 - x)
	# 3 + 3x + 5 - 5x + 7 - (1 - x)
	# 15 - 2x - (1 - x)
	# 14 - x
	f(c - b)

	# 1 + x + 1 - x + 15 - 2x
	# 17 - 2x
	=> a + b + c
}

z(x) {
	a = 1 + x
	g(a)
	g(a)
	g(a)
	g(a)
	g(a)
}

h(a, b, c) {
	=> (a + b) * c
}

j(a, x) {
	=> a * x + a * x
}

k(x) => !x + j(x, x) + !x

loops_1(a) {
	x = a + 1 # Assigned
	y = 0

	loop {
		y += x - 1 # OK
	}

	=> y
}

loops_2(a) {
	x = a + 1 # Not assigned
	y = 0

	loop {
		y += x - 1
		x += 1
	}

	=> y
}

loops_3(a) {
	x = a + 1 # Removed
	y = 0

	loop {
		x = 1 # Assigned
		y += x - 1
	}

	=> y
}

loops_4(a) {
	x = a + 1 # Removed
	y = 0

	loop {
		x = 1 # Assigned
		y += x - 1 
	}

	=> x # = 1
}

loops_5(a) {
	x = a + 1 # Removed
	y = 0

	loop {
		x = 1 # Assigned
		y += x - 1

		loop {
			y += x
		}
	}

	=> x # = 1
}

statement_1(a) {
	x = a + 1

	if (a < 0) {
		x = 1
	}

	=> x # = ?
}

statement_2(a) {
	x = a + 1

	if a < 0 {
		x = 1
	}
	else a == 0 {
		x = -1
	}

	=> x # = ?
}

statement_3(a) {
	x = a + 1

	if a < 0 {
		if a > -10 {
			if a == -1 {
				x = -1
			}
		}
	}

	=> x # = ?
}

statement_4(a) {
	x = a + 1

	loop {
		x = 1
	}

	=> x # = 1
}

statement_5(a) {
	x = a + 1

	loop {
		loop {
			loop {
				x = 1
			}
		}
	}

	=> x # = 1
}

statement_6(a) {
	x = a + 1

	loop (a < 0) {
		x = 1
		++a
	}

	=> x # = ?
}

statement_7(a) {
	x = a + 1

	loop (a < 0) {
		loop (a < -10) {
			loop (a < -20) {
				x = 1
				++a
			}
			++a
		}
		++a
	}

	=> x # = ?
}

statement_8(a) {
	if 2 * a > a + 1 {
		=> 1
	}

	=> 0
}

precomputation_1(a) {
	if a == a {
		=> 1
	}

	=> 0
}

precomputation_2(a) {
	if a != a {
		=> 1
	}
	else a + 1 == a + 1 {
		=> 2
	}
	else {
		=> 3
	}
}

precomputation_3(a) {
	if a != a {
		=> 1
	}
	else a + 1 == a {
		=> 2
	}
	else {
		=> 3
	}
}

loop_unwrap_1(a) {
	loop (i = 0, i < 10, ++i) {
		a = i
	}

	=> a
}

boopbop(x: bool) {
	if !x {
		=> 1
	}

	=> 0
}

# a^2 * x^2 + ax + 1 => ax * (ax + 1) + 1 => s = ax \ s * (s + 1) + 1
polynomial(a, x) {
	=> a * a * x * x + a * x + 1
}

aliasing_1(x) {
	a = x
	b = a

	r = 0
	
	loop (i = 0, i < 2 * x, ++i) {
		r += a
	}

	=> r + b
}

aliasing_2(x) {
	a = x
	x += 1
	=> a + x
}

decimal_comparison(a: decimal, b: decimal) {
	if a > b {
		=> a + b
	}
	else a < b {
		=> a - b
	}
	else {
		=> 0.0
	}
}

init() {
	g(10)
	z(10)
	h(1, 1, 1)
	j(1, 1)
	k(1)

	loops_1(1)
	loops_2(1)
	loops_3(1)
	loops_4(1)
	loops_5(1)
	polynomial(1, 1)

	statement_1(1)
	statement_2(1)
	statement_3(1)
	statement_4(1)
	statement_5(1)
	statement_6(1)
	statement_7(1)
	statement_8(1)
	
	precomputation_1(1)
	precomputation_2(1)
	precomputation_3(1)

	loop_unwrap_1(1)
	boopbop(1 as bool)

	aliasing_1(1)
	aliasing_2(1)
	decimal_comparison(1, 1)
}