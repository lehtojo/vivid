Foo {
	x: large
}

export encode_case_1() {
	1 + 2
}

export encode_case_2(a: large) {
	a + 1
}

export encode_case_3(a: large, b: large) {
	a + b
}

export encode_case_4(a: large) {
	1 + a
}

export encode_case_5() {
	1 - 2
}

export encode_case_6(a: large) {
	a - 1
}

export encode_case_7(a: large, b: large) {
	a - b
}

export encode_case_8(a: large) {
	1 - a
}

export encode_memory_case_1(data: link, value: large) {
	data[0] = value
}

export encode_memory_case_2(data: link<small>, value: large) {
	data[0] = value
}

export encode_memory_case_3(data: link<normal>, value: large) {
	data[0] = value
}

export encode_memory_case_4(data: link<large>, value: large) {
	data[0] = value
}

export encode_indexed_memory_case_1(data: link, i: large, value: large) {
	data[i] = value
}

export encode_indexed_memory_case_2(data: link<small>, i: large, value: large) {
	data[i] = value
}

export encode_indexed_memory_case_3(data: link<normal>, i: large, value: large) {
	data[i] = value
}

export encode_indexed_memory_case_4(data: link<large>, i: large, value: large) {
	data[i] = value
}

export encode_return_constant() {
	=> 10000
}

export encode_jumps_1(a: large, b: large) {
	if a > b => a
	else => b
}

export encode_jumps_2(a: large, b: large) {
	encode_case_1()
}

export encode_call_external() {
	=> allocate(100)
}

#export sandbox() {
#	a = 1 + 2
#	b = 7 - 3
#	d = a - b
#	c = a + b
#}

export load(foo: Foo, i: large, data: link<large>) {
	data = 1
	i = 2
	=> foo.x + data[i]
}

init() {
	foo = Foo()
	=> 1
}