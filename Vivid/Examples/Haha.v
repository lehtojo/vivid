Object {
	x: large
}

export case_1(foo: Object, memory: link) {
	foo.x = 1
	memory[0] = 2
	=> foo.x
}

export case_2(foo: Object, goo: Object) {
	foo.x = 1
	goo.x = 2
	=> foo.x
}

export case_3(memory: link) {
	memory[0] = 2
	=> memory[0]
}

export g(x: large) {
	=> x * x
}

export hoo(i: large) {
	=> i++ + ++i + i++
}

export create_object() {
	=> Object()
}

export qoo(i: large, object: Object) {
	=> object.x-- + i + i++ + ++i + create_object().x++
}

init() {
	=> g(1) + g(2)
}