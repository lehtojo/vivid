[section .text]

extern VirtualAlloc
extern VirtualFree
extern Sleep

global _V5sleepx:function
_V5sleepx:
jmp Sleep

; rcx: Bytes
global _V8allocatex_rPh:function
_V8allocatex_rPh:

mov rdx, rcx ; Bytes
xor rcx, rcx ; lpAddress
mov r8, 0x00001000 | 0x00002000
mov r9, 0x04 ; PAGE_READWRITE

sub rsp, 40
call VirtualAlloc
add rsp, 40

ret

global _V10deallocatePhx:function
_V10deallocatePhx:

; rcx = lpAddress
xor rdx, rdx ; dwSize
mov r8, 0x00008000 ; MEM_RELEASE

sub rsp, 40
call VirtualFree
add rsp, 40

ret

; rcx: Source
; rdx: Count
; r8: Destination
global _V4copyPhxPS_:function
_V4copyPhxPS_:
push rdi
push rsi

mov rdi, r8 ; Destination
mov rsi, rcx ; Source
mov rcx, rdx ; Count

rep movsb

pop rsi
pop rdi
ret

; rcx: Source
; rdx: Count
; r8: Destination
; r9: Offset
global _V11offset_copyPhxPS_x:function
_V11offset_copyPhxPS_x:
push rdi
push rsi

mov rdi, r8 ; Destination
add rdi, r9 ; Apply offset
mov rsi, rcx ; Source
mov rcx, rdx ; Count

rep movsb

pop rsi
pop rdi
ret

; rcx: Destination
; rdx: Count
global _V4zeroPhx:function
_V4zeroPhx:
push rdi

mov rdi, rcx ; rdi = Destination
mov rcx, rdx ; rcx = Count
xor rax, rax ; Value used to fill the range

rep stosb

pop rdi
ret

; rcx: Destination
; rdx: Count
; r8: Value
global _V4fillPhxx:function
_V4fillPhxx:
push rdi

mov rdi, rcx ; rdi = Destination
mov rcx, rdx ; rcx = Count
mov rax, r8 ; rax = Value

rep stosb

pop rdi
ret