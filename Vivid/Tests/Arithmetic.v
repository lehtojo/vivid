export arithmetic(a, b, c) {
	x = a * c + a + c
	y = b * a * (c + 1) * 100
	=> x + y
}

export addition(a, b) {
	=> a + b
}

export subtraction(a, b) {
	=> a - b
}

export multiplication(a, b) {
	=> a * b
}

export division(a, b) {
	=> a / b
}

export remainder(a, b) {
	=> a % b
}

export operator_order(a, b) {
	=> a + b * a - b / a
}

export addition_with_constant(a) {
	=> 10 + a + 10
}

export subtraction_with_constant(a) {
	=> -10 + a - 10
}

export multiplication_with_constant(a) {
	=> 10 * a * 10
}

export division_with_constant(a) {
	=> 100 / a / 10
}

export preincrement(a) {
	=> ++a + 7
}

export predecrement(a) {
	=> --a + 7
}

export postincrement(a) {
	=> a++ + 3
}

export postdecrement(a) {
	=> a-- + 3
}

export increments(a) {
	=> a + a++ * ++a + a
}

export decrements(a) {
	=> a + a-- * --a + a
}

init() {
	=> 1
	addition(0, 0)
	subtraction(0, 0)
	multiplication(0, 0)
	division(1, 1)
	addition_with_constant(0)
	subtraction_with_constant(0)
	multiplication_with_constant(0)
	division_with_constant(0)
	arithmetic(1, 2, 3)
	preincrement(1)
	predecrement(1)
	postincrement(1)
	postdecrement(1)
	increments(1)
	decrements(1)
}