section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh
extern _V10deallocatePhx
extern _V3cosd_rd
extern _V3sind_rd
extern _V5sleepx
extern _V4fillPhxx
extern _V14internal_printPhx

_V4initv_rx:
push rbx
push rsi
push rdi
sub rsp, 144
mov rcx, 1760
imul rcx, 8
call _V8allocatex_rPh
mov rcx, 1760
mov rbx, rax
call _V8allocatex_rPh
lea rcx, [rel _V4initv_rx_S0]
mov rsi, rax
call _V5printPh
pxor xmm0, xmm0
pxor xmm1, xmm1
_V4initv_rx_L1:
_V4initv_rx_L0:
mov rcx, rsi
mov rdx, 1760
mov r8, 32
movsd qword [rsp+136], xmm0
movsd qword [rsp+128], xmm1
call _V4fillPhxx
mov rdx, 1760
imul rdx, 8
mov rcx, rbx
xor r8, r8
call _V4fillPhxx
pxor xmm0, xmm0
movsd xmm1, qword [rel _V4initv_rx_C0]
comisd xmm0, xmm1
jae _V4initv_rx_L4
_V4initv_rx_L3:
pxor xmm1, xmm1
movsd xmm2, qword [rel _V4initv_rx_C0]
comisd xmm1, xmm2
jae _V4initv_rx_L7
_V4initv_rx_L6:
movsd xmm2, xmm0
movsd xmm0, xmm1
movsd qword [rsp+120], xmm1
movsd qword [rsp+112], xmm2
call _V3sind_rd
movsd xmm1, xmm0
movsd xmm0, qword [rsp+112]
movsd qword [rsp+104], xmm1
call _V3cosd_rd
movsd xmm1, xmm0
movsd xmm0, qword [rsp+136]
movsd qword [rsp+96], xmm1
call _V3sind_rd
movsd xmm1, xmm0
movsd xmm0, qword [rsp+112]
movsd qword [rsp+88], xmm1
call _V3sind_rd
movsd xmm1, xmm0
movsd xmm0, qword [rsp+136]
movsd qword [rsp+80], xmm1
call _V3cosd_rd
movsd xmm1, qword [rsp+96]
movsd xmm2, xmm1
movsd xmm3, qword [rel _V4initv_rx_C1]
addsd xmm2, xmm3
movsd xmm3, qword [rsp+104]
movsd xmm4, xmm3
mulsd xmm4, xmm2
movsd xmm5, qword [rsp+88]
mulsd xmm4, xmm5
movsd xmm6, qword [rsp+80]
movsd xmm7, xmm6
mulsd xmm7, xmm0
addsd xmm4, xmm7
movsd xmm7, qword [rel _V4initv_rx_C2]
addsd xmm4, xmm7
movsd xmm7, qword [rel _V4initv_rx_C3]
divsd xmm7, xmm4
movsd xmm4, xmm0
movsd xmm0, qword [rsp+120]
movsd qword [rsp+96], xmm1
movsd qword [rsp+72], xmm2
movsd qword [rsp+104], xmm3
movsd qword [rsp+64], xmm4
movsd qword [rsp+88], xmm5
movsd qword [rsp+80], xmm6
movsd qword [rsp+56], xmm7
call _V3cosd_rd
movsd xmm1, xmm0
movsd xmm0, qword [rsp+128]
movsd qword [rsp+48], xmm1
call _V3cosd_rd
movsd xmm1, xmm0
movsd xmm0, qword [rsp+128]
movsd qword [rsp+40], xmm1
call _V3sind_rd
movsd xmm1, qword [rsp+104]
movsd xmm2, xmm1
movsd xmm3, qword [rsp+72]
mulsd xmm2, xmm3
movsd xmm4, qword [rsp+64]
mulsd xmm2, xmm4
movsd xmm5, qword [rsp+80]
movsd xmm6, xmm5
movsd xmm7, qword [rsp+88]
mulsd xmm6, xmm7
subsd xmm2, xmm6
movsd xmm6, qword [rel _V4initv_rx_C4]
movsd xmm8, qword [rsp+56]
mulsd xmm6, xmm8
movsd xmm9, qword [rsp+48]
movsd xmm10, xmm9
mulsd xmm10, xmm3
movsd xmm11, qword [rsp+40]
mulsd xmm10, xmm11
movsd xmm12, xmm2
mulsd xmm12, xmm0
subsd xmm10, xmm12
mulsd xmm6, xmm10
movsd xmm10, qword [rel _V4initv_rx_C5]
addsd xmm10, xmm6
cvttsd2si rcx, xmm10
movsd xmm6, qword [rel _V4initv_rx_C6]
mulsd xmm6, xmm8
movsd xmm10, xmm9
mulsd xmm10, xmm3
mulsd xmm10, xmm0
movsd xmm12, xmm2
mulsd xmm12, xmm11
addsd xmm10, xmm12
mulsd xmm6, xmm10
movsd xmm10, qword [rel _V4initv_rx_C7]
addsd xmm10, xmm6
cvttsd2si rdx, xmm10
mov r8, 80
imul r8, rdx
lea r9, [rcx+r8]
movsd xmm6, xmm5
mulsd xmm6, xmm7
movsd xmm10, xmm1
movsd xmm12, qword [rsp+96]
mulsd xmm10, xmm12
mulsd xmm10, xmm4
subsd xmm6, xmm10
mulsd xmm6, xmm11
movsd xmm10, xmm1
mulsd xmm10, xmm12
mulsd xmm10, xmm7
subsd xmm6, xmm10
movsd xmm10, xmm5
mulsd xmm10, xmm4
subsd xmm6, xmm10
movsd xmm10, xmm9
mulsd xmm10, xmm12
mulsd xmm10, xmm0
subsd xmm6, xmm10
movsd xmm10, qword [rel _V4initv_rx_C8]
mulsd xmm10, xmm6
cvttsd2si r8, xmm10
mov r10, 22
cmp r10, rdx
jle _V4initv_rx_L9
test rdx, rdx
jle _V4initv_rx_L9
test rcx, rcx
jle _V4initv_rx_L9
mov r10, 80
cmp r10, rcx
jle _V4initv_rx_L9
mov r10, r9
sal r10, 3
movsd xmm6, qword [rbx+r10]
comisd xmm8, xmm6
jbe _V4initv_rx_L9
mov r10, r9
sal r10, 3
movsd qword [rbx+r10], xmm8
test r8, r8
jle _V4initv_rx_L16
lea r11, [rel _V4initv_rx_S1]
mov r10b, [r11+r8]
mov byte [rsi+r9], r10b
jmp _V4initv_rx_L15
_V4initv_rx_L16:
mov byte [rsi+r9], 46
_V4initv_rx_L15:
_V4initv_rx_L9:
movsd xmm0, qword [rsp+120]
movsd xmm1, qword [rel _V4initv_rx_C9]
addsd xmm0, xmm1
movsd xmm1, xmm0
movsd xmm0, qword [rsp+112]
movsd xmm2, qword [rel _V4initv_rx_C0]
comisd xmm1, xmm2
jb _V4initv_rx_L6
_V4initv_rx_L7:
movsd xmm1, qword [rel _V4initv_rx_C10]
addsd xmm0, xmm1
movsd xmm1, qword [rel _V4initv_rx_C0]
comisd xmm0, xmm1
jb _V4initv_rx_L3
_V4initv_rx_L4:
lea rcx, [rel _V4initv_rx_S2]
call _V5printPh
xor rdi, rdi
cmp rdi, 1761
jge _V4initv_rx_L21
_V4initv_rx_L20:
mov rax, rdi
xor rdx, rdx
mov rcx, 80
idiv rcx
test rdx, rdx
je _V4initv_rx_L24
movzx rcx, byte [rsi+rdi]
call _V15print_characterx
jmp _V4initv_rx_L23
_V4initv_rx_L24:
mov rcx, 10
call _V15print_characterx
_V4initv_rx_L23:
movsd xmm0, qword [rsp+136]
movsd xmm1, qword [rel _V4initv_rx_C11]
addsd xmm0, xmm1
movsd xmm1, qword [rsp+128]
movsd xmm2, qword [rel _V4initv_rx_C12]
addsd xmm1, xmm2
add rdi, 1
movsd qword [rsp+136], xmm0
movsd qword [rsp+128], xmm1
cmp rdi, 1761
jl _V4initv_rx_L20
_V4initv_rx_L21:
mov rcx, 2
call _V5sleepx
movsd xmm0, qword [rsp+136]
movsd xmm1, qword [rsp+128]
jmp _V4initv_rx_L0
_V4initv_rx_L2:
xor rax, rax
add rsp, 144
pop rdi
pop rsi
pop rbx
ret

_V5printPh:
push rbx
sub rsp, 48
mov rbx, rcx
call _V9length_ofPh_rx
mov rcx, rbx
mov rdx, rax
call _V14internal_printPhx
add rsp, 48
pop rbx
ret

_V15print_characterx:
push rbx
sub rsp, 48
mov rbx, rcx
mov rcx, 1
call _V8allocatex_rPh
mov byte [rax], bl
mov rcx, rax
mov rdx, 1
mov rbx, rax
call _V14internal_printPhx
mov rcx, rbx
mov rdx, 1
call _V10deallocatePhx
add rsp, 48
pop rbx
ret

_V9length_ofPh_rx:
xor rax, rax
_V9length_ofPh_rx_L1:
_V9length_ofPh_rx_L0:
movzx rdx, byte [rcx+rax]
test rdx, rdx
jne _V9length_ofPh_rx_L3
ret
_V9length_ofPh_rx_L3:
add rax, 1
jmp _V9length_ofPh_rx_L0
_V9length_ofPh_rx_L2:
ret

section .data

align 16
_V4initv_rx_S0 db 27, '[2J', 0
align 16
_V4initv_rx_S1 db '.,-~:;=!*#$@', 0
align 16
_V4initv_rx_S2 db 27, '[H', 0
align 16
_V4initv_rx_C0 db 31, 133, 235, 81, 184, 30, 25, 64 ; 6.28
align 16
_V4initv_rx_C1 db 0, 0, 0, 0, 0, 0, 0, 64 ; 2.0
align 16
_V4initv_rx_C2 db 0, 0, 0, 0, 0, 0, 20, 64 ; 5.0
align 16
_V4initv_rx_C3 db 0, 0, 0, 0, 0, 0, 240, 63 ; 1.0
align 16
_V4initv_rx_C4 db 0, 0, 0, 0, 0, 0, 62, 64 ; 30.0
align 16
_V4initv_rx_C5 db 0, 0, 0, 0, 0, 0, 68, 64 ; 40.0
align 16
_V4initv_rx_C6 db 0, 0, 0, 0, 0, 0, 46, 64 ; 15.0
align 16
_V4initv_rx_C7 db 0, 0, 0, 0, 0, 0, 40, 64 ; 12.0
align 16
_V4initv_rx_C8 db 0, 0, 0, 0, 0, 0, 32, 64 ; 8.0
align 16
_V4initv_rx_C9 db 123, 20, 174, 71, 225, 122, 148, 63 ; 0.02
align 16
_V4initv_rx_C10 db 236, 81, 184, 30, 133, 235, 177, 63 ; 0.07
align 16
_V4initv_rx_C11 db 241, 104, 227, 136, 181, 248, 4, 63 ; 4E-05
align 16
_V4initv_rx_C12 db 241, 104, 227, 136, 181, 248, 244, 62 ; 2E-05