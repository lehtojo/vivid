Foo {
	x: large
	y: large

	init(x: large, y: large) {
		this.x = x
		this.y = y
	}

	###
	init(this.x, this.y) {}
	###
}

###
anonymous_pack_1(options: { x: large, y: large }) {
	=> options.x + options.y
}
###

export anonymous_pack_2(x: large, y: large) {
	=> { x: y, y: x }
}


pack ManualOptions {
	x: large
	y: large

	init(x: large, y: large) {
		this.x = x
		this.y = y
	}
}

export pack_1(options: ManualOptions) {
	=> options.x + options.y
}

export pack_2(x: large, y: large) {
	options: ManualOptions
	options.x = y
	options.y = x
	=> options
}

compute(foos: List<Foo>) {
	sum = 0

	loop foo in foos {
		sum += foo.x + foo.y
	}

	=> sum
}

init() {
	println(compute([ Foo(1, 2), Foo(3, 4) ]))
	=> 0
}

###
init() {
	foos = List<Foo>()
	foos.add(Foo(1, 2))
	foos.add(Foo(3, 4))

	println(compute(foos))
	=> 0
}
###