pack Foo {
	x: large
	y: small
}

# Test: Create an unnamed pack and return it
pack_1(x: large, y: small) {
	return pack {
		x: y as large,
		y: x as small
	}
}

# Test: Receiving a unnamed pack and using it
pack_2(a: { x: large, y: small }) {
	return a.x * a.y
}

# Test: Convert a named pack to unnamed pack
pack_3(x: large, y: small) {
	foo: Foo
	foo.x = x
	foo.y = y
	return pack_2(foo)
}

# Test: Create nested packs
pack_4(x: large, y: small, z: normal, w: tiny) {
	return pack {
		i: pack {
			x: x * 3,
			y: (y * 5) as small
		},
		j: pack {
			z: (z * 7) as normal,
			w: (w * 11) as tiny
		}, # The comma is intentional, because the compiler should not care about it
	}
}

# Test: Receive nested packs
pack_5(a: { i: { x: large, y: small }, j: { z: normal, w: tiny } }) {
	return pack_2(a.i) + a.j.z * a.j.w
}

init() {
	a = pack_1(10, 42)
	console.write_line(pack_2(a))
	console.write_line(pack_3(-42, -10))
	b = pack_4(11, 7, 5, 3)
	console.write_line(pack_5(b))
	return 0
}