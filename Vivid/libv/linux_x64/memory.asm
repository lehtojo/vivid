[section .text]

; rdi: Length
global _V8allocatex_rPh:function hidden
_V8allocatex_rPh:
;   System call parameters
;
;   off_t offset,
;   int fd, 
;   int flags,	
;   int prot, 
;   size_t length, 
;   void *addr
;
mov rsi, rdi ; Length
xor rdi, rdi ; Address
mov rdx, 0x03 ; PERMISSION_READ | PERMISSION_WRITE
mov r10, 0x22 ; HARDWARE_MEMORY | VISIBILITY_PRIVATE
mov r8, -1 ; FD
xor r9, r9

; System call: mmap
mov rax, 0x09
syscall

ret


; rdi: Address
; rsi: Length
global _V10deallocatePhx:function hidden
_V10deallocatePhx:

mov rax, 11 ; munmap
syscall

ret

; rdi: Source
; rsi: Count
; rdx: Destination
global _V4copyPhxPS_:function hidden
_V4copyPhxPS_:

xchg rdi, rdx ; rdi = Destination, rdx = Source
mov rcx, rsi ; rcx = Count
mov rsi, rdx ; rsi = Source

rep movsb

ret

; rdi: Source
; rsi: Count
; rdx: Destination
; rcx: Offset
global _V11offset_copyPhxPS_x:function hidden
_V11offset_copyPhxPS_x:

add rdx, rcx ; Apply offset

xchg rdi, rdx ; rdi = Destination, rdx = Source
mov rcx, rsi ; rcx = Count
mov rsi, rdx ; rsi = Source

rep movsb

ret

; rdi: Destination
; rsi: Count
global _V4zeroPhx:function hidden
_V4zeroPhx:

mov rcx, rsi ; rcx = Count
xor rax, rax ; Value used to fill the range

rep stosb

ret

; rdi: Destination
; rsi: Count
; rdx: Value
global _V4fillPhxx:function hidden
_V4fillPhxx:

mov rcx, rsi ; rcx = Count
mov rax, rdx ; rax = Value

rep stosb

ret

; rdi: code
global _V4exitx:function hidden
_V4exitx:
mov rax, 60
syscall
jmp _V4exitx