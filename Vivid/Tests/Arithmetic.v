export arithmetic(a, b, c) {
	x = a * c + a + c
	y = b * a * (c + 1) * 100
	return x + y
}

export addition(a, b) {
	return a + b
}

export subtraction(a, b) {
	return a - b
}

export multiplication(a, b) {
	return a * b
}

export division(a, b) {
	return a / b
}

export remainder(a, b) {
	return a % b
}

export operator_order(a, b) {
	return a + b * a - b / a
}

export addition_with_constant(a) {
	return 10 + a + 10
}

export subtraction_with_constant(a) {
	return -10 + a - 10
}

export multiplication_with_constant(a) {
	return 10 * a * 10
}

export division_with_constant(a) {
	return 100 / a / 10
}

export preincrement(a) {
	return ++a + 7
}

export predecrement(a) {
	return --a + 7
}

export postincrement(a) {
	return a++ + 3
}

export postdecrement(a) {
	return a-- + 3
}

export increments(a) {
	return a + a++ * ++a + a
}

export decrements(a) {
	return a + a-- * --a + a
}

init() {
	return 1
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