sum(a, b) => a + b
sub(a, b) => a - b
mul(a, b, c, d, e) => a * b * c * d * e

power(a, b) {
	if a > b {
		=> sum(a, b)
	}
	else {
		=> mul(a, a, a, a, a)
	}
}

init() {
	c = power(1, 2)
	=> c
}