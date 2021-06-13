Type {
	a: large
	b: large

	static c: large

	sub(x, y) {
		if x < y => 0
		=> x - y
	}

	sum(x, y) {
		if x * y > a * b {
			if x > a and y > b {
				loop (i = 0, i < a, i++) {
					=> x * y * a * b * i
				}
			}

			=> a + b + sub(x, y)
		}

		=> x * y
	}
}

export yaa(type: Type, x: large, y: large) {
	=> type.sum(x, y)
}

export letsgo(a, b) {
	if a > b or Type.c == 7 => 3 * a
	a *= 2
	=> a + b
}

export is_alphabet(value: char) {
	=> (value >= `a` and value <= `z`) or (value >= `A` and value <= `Z`)
}

export case_1(a: large, b: large) {
	if a > b {
		=> a + b
	}
	else {
		=> a - b
	}
}

export case_2(a: large, b: large) {
	=> case_1(a, b)
}

init() {
	a = 1
	=> letsgo(a, 2) + a
}