###
h(a) => (a[0] as num) * 11
bar(a) => a * 11

foo(a, c, d) {
    if a > 10 {
        b = bar(a)
        a = b + 1
        c = b - 1
    }

    => a
}

goo(a, b) {
    bar(b)
    => a + b
}

init() {
    h(0 as link)
    => foo(0, 0, 0) + goo(0, 0)
}
###

export branch_1(x) {
	a = 10

	if x > 0 {
		a = -1
	}

	=> a + x
}

export branch_2(x) {
	a = 10

	if x > 0 {
		a = -1
	}
	else x == 0 {
		a = 0
	}

	=> a + x
}

export branch_3(x) {
	a = 10
	b = 10

	if x > 0 {
		b = 0
	}
	else x == 0 {
		b = 0
	}
	else {
		a = -1
	}

	=> a + x
}

export branch_4(x) {
	a = 10

	if x > 0 {
		a = 1
	}
	else x == 0 {
		a = 2
	}
	else {
		a = 3
	}

	=> a + x
}

export branch_5(x) {
	a = 0

	loop (i = 0, i < x, i++) {
		a = i
	}

	=> a
}

export dependency_1(x) {
	a = x + 1
	x++
	=> a
}

init() {
	branch_1(0)
	branch_2(0)
	branch_3(0)
	branch_4(0)
	branch_5(0)
	dependency_1(0)
	=> 1 + 2
}