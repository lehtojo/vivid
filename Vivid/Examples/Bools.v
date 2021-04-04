import large_function()

export bool_1(x: decimal, y: decimal) {
	is_larger = x > y
	=> is_larger
}

export bool_2(x: large, y: large, a: large) {
	
	if x > y {
		a = 1
	}

	=> a
}

export bool_3(x: large, y: large, a: large) {
	if x > y {
		a = 1
	}
	else {
		a = 2
	}

	=> a
}

export bool_4(x: large, y: large, a: large) {
	if x > y {
		a = x + y
	}
	else {
		a = x * y
	}

	=> a
}

export bool_5(x: large, y: large, a: large) {
	if x > y {
		large_function()
		a = x + y
	}
	else {
		a = x * y
	}

	=> a
}

export bool_6(x: decimal, y: decimal, a: large, b: large) {
	if x > y {
		a = 1
	}
	else {
		a = b * b
	}

	=> a

}

export bool_7(a: large, b: large, x: large, y: large) {
	i = x * y
	j = x + y

	if a == b {
		i = x + x
		j = y * y
	}
	else {
		i = x + y
		j = x * y
	}

	=> i + j
}

export cpy(s: link, d: link, n: large) {
	loop (i = 0, i <= n - 8, i += 8) {
		d[0] = s[0]
		d[1] = s[1]
		d[2] = s[2]
		d[3] = s[3]
		d[4] = s[4]
		d[5] = s[5]
		d[6] = s[6]
		d[7] = s[7]

		d[8] = s[8]
		d[9] = s[9]
		d[10] = s[10]
		d[11] = s[11]
		d[12] = s[12]
		d[13] = s[13]
		d[14] = s[14]
		d[15] = s[15]
	}
}

get(d: link) {
	d[42] = 42
	=> 1
}

export unwrap_1(d: link, r: large) {
	loop (i = 0, get(d) > 2, i++) {
		r++
	}

	=> r
}

###
export collection_1() => { 1, 2, 3, 4 }
export collection_2(i: large) => { i * i, i + i, i - i, i / i }

export operator_cast_1(i: large) => { i * i, i + i, i - i, i / i } as List<large>.first()

export foreach_1(numbers: collection<large>) {
	r = 0

	loop i in collection {
		r += i
	}

	=> r
}

export foreach_2() {
	r = 0

	loop i in { 1, 2, 3, 4 } {
		r += i
	}

	=> r
}

export range_1(r: large, s: large, e: large) {
	loop i in -10..17 {
		r += i
	}

	loop i in s..e {
		r += i
	}

	=> r
}

export not_1(a: large, b: large) {
	if not a > b {
		=> -1
	}

	=> a + b
}###

export fooq(a: large, b: large) {
	=> a / a
}

init() {
	bool_6(2.0, 1.0, 0, 2)
	bool_6(1.0, 2.0, 0, 2)
	=> 0
}