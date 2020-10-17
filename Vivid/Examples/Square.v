Alpha {
	x: num
	y: num
}

Beta {
	x: num
	y: num
}

foo(a: Alpha, b: Alpha, c: Beta, d: Alpha, e: num, f: Beta) {
	=> 0
}

init() {
	foo(Alpha(), Alpha(), Beta(), Alpha(), 0, Beta())
}