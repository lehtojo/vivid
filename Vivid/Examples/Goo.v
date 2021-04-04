Object {
	a: decimal
	b: decimal
	other: Object

	deinit() {
		println('Tuhottu boi!')
	}
}

Holder {
	object: Object
}

create_holder() {
	holder = Holder()
	holder.object = Object()
	=> holder
}

outline export haha() {
	object = Object()
}

init() {
	holder = create_holder()
	holder.object = Object()
	=> true
}

sum(a, b) => a + b
mul(a, b) => a * b

export hoo(a: large, b: large) {
	r = 0

	if a > b {
		r += sum(a, b)
	}
	else {
		r += mul(a, b)
	}

	=> r
}

###
export test_1(object: Object) {
	=> object.a
}

export test_2(a: large, b: large) {
	object = Object()
	object.a = a
	object.b = b
}


export test_3(a: large, b: large) {
	object = Object()
	object.a = a
	object.b = b
	=> object
}


export test_4(object: Object, holder: Holder) {
	holder.object = object
}

export test_5(holder: Holder) {
	object = holder.object
	holder.object = Object()
	=> object.a
}

export test_6(a: Holder, b: Holder) {
	a.object = b.object
}

export test_7(object: Object) {
	object.a = Object().a
}

export test_8(holder: Holder) {
	test_1(holder.object)
}

export indirect(object: Object) {
	=> object
}

export test_9() {
	object = Object()
	=> indirect(object)
}

export test_10(a: large) {
	if a > 0 {
		object = Object()
		object.a = 10.0
		=> object.a
	}

	=> 10.0
}

init() => true

# X Jos muuttuja ladataan muistista sijoituksen yhteydessä, sen viittausten määrä kasvaa
# X Jos palautetaan paikallinen muuttuja, sen viittausten määrät eivät saa muuttua
# X Jos palautusarvona on funktio, viittausten määrä ei kasva
# X Sijoitettavan arvon viittausten määrä ei kasva, kun se on funktio (viittausten määrä on jo kasvanut palautusvaiheessa)
# X Sijoittessa muuttujaan, jota ei julisteta, sijoituskohteen viittausten määrä laskee
# Palautusarvolle, jota ei sijoiteta, muodostetaan sijoitusmuuttuja, joka tuhotaan näkyvyysalueen lopussa
###