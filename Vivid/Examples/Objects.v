Base {
	name: link
}

Base Object {
	x: large
	other: Object

	deinit() {
		println('Object is destroyed')
	}
}

Base Ubject {
	x: large

	deinit() {
		println('Ubject is destroyed')
	}
}

export increment(object: Object) {
	object.x++
}

export test_1() {
	object = Object()
	object.x = 1

	increment(object)

	=> object.x
}

create_object() {
	=> Object()
}

export test_2() {
	create_object()
}

export test_3() {
	object = Object()
	=> object
}

export test_4(object: Object) {
	=> object
}

export test_5(object: Object) {
	object.other = Object()
}

export test_6(object: Object) {
	object = Object()
}

export test_7(object: Object) {
	object = Object()
	=> object
}

export test_8(object: Object) {
	object = Object()
	object = Object()
	=> object
}

ignore(input) {}

export test_9() {
	ignore(create_object())
}

export test_10(n: large) {
	result = Object()

	loop (i = 0, i < n, i++) {
		result = Object()
	}

	=> result
}

export test_11() {
	a = Object()

	h = a

	ignore(h)
}

init() {}