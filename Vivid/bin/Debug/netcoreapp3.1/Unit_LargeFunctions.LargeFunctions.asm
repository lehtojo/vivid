.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V17internal_allocatex_rPh

.global _V1axxxxxx_rx
_V1axxxxxx_rx:
add rcx, rdx
add rcx, r8
lea rax, [rcx+r9]
add rax, [rsp+40]
add rax, [rsp+48]
ret

.global _V1xxx_rx
_V1xxx_rx:
push rbx
sub rsp, 48
lea r8, [rcx+1]
mov rbx, rcx
sar rbx, 1
sal rcx, 2
lea r10, [rdx+1]
mov r11, rdx
sal r11, 1
sar rdx, 2
mov qword ptr [rsp+40], rdx
mov r9, r10
mov qword ptr [rsp+32], r11
mov rdx, rbx
xchg r8, rcx
call _V1axxxxxx_rx
add rsp, 48
pop rbx
ret

.global _V1bxxxxxxiscd_rd
_V1bxxxxxxiscd_rd:
add rcx, rdx
add rcx, r8
add rcx, r9
add rcx, [rsp+40]
add rcx, [rsp+48]
add rcx, [rsp+56]
add rcx, [rsp+64]
add rcx, [rsp+72]
cvtsi2sd xmm1, rcx
addsd xmm1, xmm0
movsd xmm0, xmm1
ret

.global _V1yxx_rd
_V1yxx_rd:
push rbx
push rsi
push rdi
push rbp
sub rsp, 72
mov r8, rcx
sub r8, 3
mov r9, rcx
sub r9, 2
mov r10, rcx
sub r10, 1
lea r11, [rcx+1]
lea rbx, [rcx+2]
lea rsi, [rcx+3]
mov rdi, rcx
imul rdi, 42
lea rbp, [rcx*2+rcx]
imul rcx, -1
cvtsi2sd xmm0, rdx
movsd xmm1, qword ptr [rip+_V1yxx_rd_C0]
addsd xmm0, xmm1
mov qword ptr [rsp+64], rcx
mov rdx, r9
mov rcx, r8
mov r8, r10
mov r9, r11
mov qword ptr [rsp+32], rbx
mov qword ptr [rsp+40], rsi
mov qword ptr [rsp+48], rdi
mov qword ptr [rsp+56], rbp
call _V1bxxxxxxiscd_rd
add rsp, 72
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _V4initv_rx
_V4initv_rx:
sub rsp, 40
mov rcx, 1
mov rdx, 1
call _V1xxx_rx
xor rcx, rcx
xor rdx, rdx
call _V1yxx_rd
mov rax, 1
add rsp, 40
ret

.section .data

.balign 16
_V1yxx_rd_C0:
.byte 57, 180, 200, 118, 190, 159, 246, 63 # 1.414

