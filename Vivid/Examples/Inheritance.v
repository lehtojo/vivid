Token {
   private:

   type: num
   text: String

   init(type, text) {
	  this.type = type
	  this.text = text
   }

   get_type() {
	  => type
   }
}

Counter {
   usages = 0

   use() {
	  ++usages
   }
}

Counter Token ContentToken {
   capacity = 0

   init() {
	  type = 1
	  text = String('Content')
	  usages = 0
	  capacity = 0
   }

   change_capacity(to: num) {
	  capacity = to
   }
}

change_capacity(token: Token) {
   (token as ContentToken).capacity = 10
   (token as ContentToken).use()
}

init() {
   content = ContentToken()
   content.type = 0
   content.text = String('Content Token')
   content.capacity = 1

   content.use()

   type = content.get_type()

   change_capacity(content as Token)
}