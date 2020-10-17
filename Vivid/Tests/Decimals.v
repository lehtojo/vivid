export decimal_addition(a: decimal, b: decimal) {
   => a + b
}

export decimal_subtraction(a: decimal, b: decimal) {
   => a - b
}

export decimal_multiplication(a: decimal, b: decimal) {
   => a * b
}

export decimal_division(a: decimal, b: decimal) {
   => a / b
}

export decimal_operator_order(a: decimal, b: decimal) {
	=> a + b * a - b / a
}

export decimal_addition_with_constant(a: decimal) {
	=> 1.414 + a + 1.414
}

export decimal_subtraction_with_constant(a: decimal) {
	=> -1.414 + a - 1.414
}

export decimal_multiplication_with_constant(a: decimal) {
	=> 1.414 * a * 1.414
}

export decimal_division_with_constant(a: decimal) {
	=> 2.0 / a / 1.414
}

init() {
	=> 1
	decimal_addition(0.0, 0.0)
	decimal_subtraction(0.0, 0.0)
	decimal_multiplication(0.0, 0.0)
	decimal_division(1.0, 1.0)
	decimal_operator_order(1.0, 1.0)
	decimal_addition_with_constant(0.0)
	decimal_subtraction_with_constant(0.0)
	decimal_multiplication_with_constant(0.0)
	decimal_division_with_constant(0.0)
}