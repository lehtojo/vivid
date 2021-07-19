Object {
	x: normal
	y: decimal
	other: Object
}

export memory_case_1(object: Object, value: normal) {
	object.x = value
	=> object.x
}

export memory_case_2(a: link, i: normal) {
	a[i] = i + 1
	=> a[i]
}

# TODO: Does not work on Windows, add second parameter 'empty: decimal'
export memory_case_3(object: Object, value: decimal) {
	object.y++
	object.x = value
	=> object.x + object.y
}

export memory_case_4(a: Object, b: Object) {
	a.x = 1
	b.x = 2
	=> a.x
}

export memory_case_5(a: Object, b: link) {
	a.y = 123.456
	b[5] = 7
	=> a.y
}

export memory_case_6(a: Object) {
	a.other.y = -3.14159
	=> a.other.y
}

export memory_case_7(a: Object, other: Object) {
	a.other.y = -3.14159
	a.other = other
	=> a.other.y
}

export memory_case_8(a: Object, other: Object) {
	a.other.y = -3.14159
	a = other
	=> a.other.y
}

export memory_case_9(a: Object, other: Object) {
	a.other.y = -3.14159
	other.y = 10
	=> a.other.y
}

export memory_case_10(a: Object, other: Object) {
	a.other.y = -3.14159
	other.other = other
	=> a.other.y
}

export memory_case_11(a: Object, i: large) {
	if i > 0 {
		a.x += 1
		a.other.x += 1
	}
	else {
		a.x += 1
		a.other.x += 1
	}
}

export memory_case_12(a: Object, i: large) {
	if i > 0 {
		a.x = a.y
	}
	else {
		a.y = a.x
	}

	=> a.x
}

export memory_case_13(a: Object, i: large) {
	if i > 0 {
		a.x += i
		a.y += i
		a.y += 1
	}
	else {
		a.x += i
		a.y += i
		a.y += 1
	}
}

init() => true