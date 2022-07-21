a(a, b, c, d, e, f) {
   return a + b + c + d + e + f
}

export x(a: large, b: large) {
	# a + 1 + 0.5a + 4a + b + 1 + 2b + 0.25b
	# 5.5a + 3.25b + 2
	return a(a + 1, a / 2, a * 4, b + 1, b * 2, b / 4)
}

b(a: large, b: large, c: large, d: large, e: large, f: large, g: normal, h: small, i: tiny, j: decimal) {
	return a + b + c + d + e + f + g + h + i + j
}

export y(a: large, b: large) {
	return b(a - 3, a - 2, a - 1, a + 1, a + 2, a + 3, a * 42, a * 3, a * -1, b + 1.414)
}

init() {
	x(1, 1)
	y(0, 0)
	return 1
}