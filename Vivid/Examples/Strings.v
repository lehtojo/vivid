Holder {
   i: num
}

init() {
   hello = string('Hello ')
   world = string('world!')

   printsln(hello + world)

   holder = Holder()
   holder[0][1][2][3] = 100
}