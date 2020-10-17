memcpy(destination, source, count) {
   i = 0

   loop (i, i < count, ++i) {
	  destination[i] = source[i]
   }
}

init() {
   memcpy(0, 0, 100)
}