[section .text]

extern VirtualAlloc
extern VirtualFree

; rcx: Bytes
global allocate:function
allocate:

mov rdx, rcx ; Bytes
xor rcx, rcx ; lpAddress
mov r8, 0x00001000 | 0x00002000
mov r9, 0x04 ; PAGE_READWRITE

sub rsp, 40
call VirtualAlloc
add rsp, 40

ret

global deallocate:function
deallocate:

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
global copy:function
copy:
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
global offset_copy:function
offset_copy:
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
global zero:function
zero:
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
global fill:function
fill:
push rdi

mov rdi, rcx ; rdi = Destination
mov rcx, rdx ; rcx = Count
mov rax, r8 ; rax = Value

rep stosb

pop rdi
ret