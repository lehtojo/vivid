.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V17internal_allocatex_rPh

.global _V11bitwise_andcc_rc
_V11bitwise_andcc_rc:
and rcx, rdx
mov rax, rcx
ret

.global _V11bitwise_xorcc_rc
_V11bitwise_xorcc_rc:
xor rcx, rdx
mov rax, rcx
ret

.global _V10bitwise_orcc_rc
_V10bitwise_orcc_rc:
or rcx, rdx
mov rax, rcx
ret

.global _V13synthetic_andcc_rc
_V13synthetic_andcc_rc:
mov rax, rcx
xor rax, rdx
not rax
or rcx, rdx
not rcx
xor rax, rcx
ret

.global _V13synthetic_xorcc_rc
_V13synthetic_xorcc_rc:
mov rax, rcx
or rax, rdx
and rcx, rdx
not rcx
and rax, rcx
ret

.global _V12synthetic_orcc_rc
_V12synthetic_orcc_rc:
mov rax, rcx
xor rax, rdx
and rcx, rdx
xor rax, rcx
ret

.global _V18assign_bitwise_andx_rx
_V18assign_bitwise_andx_rx:
mov rdx, rcx
sar rdx, 1
and rcx, rdx
mov rax, rcx
ret

.global _V18assign_bitwise_xorx_rx
_V18assign_bitwise_xorx_rx:
xor rcx, 1
mov rax, rcx
ret

.global _V17assign_bitwise_orxx_rx
_V17assign_bitwise_orxx_rx:
or rcx, rdx
mov rax, rcx
ret

.global _V4initv_rx
_V4initv_rx:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
xor rcx, rcx
xor rdx, rdx
call _V11bitwise_andcc_rc
xor rcx, rcx
xor rdx, rdx
call _V11bitwise_xorcc_rc
xor rcx, rcx
xor rdx, rdx
call _V10bitwise_orcc_rc
xor rcx, rcx
xor rdx, rdx
call _V13synthetic_andcc_rc
xor rcx, rcx
xor rdx, rdx
call _V13synthetic_xorcc_rc
xor rcx, rcx
xor rdx, rdx
call _V12synthetic_orcc_rc
xor rcx, rcx
call _V18assign_bitwise_andx_rx
xor rcx, rcx
call _V18assign_bitwise_xorx_rx
xor rcx, rcx
xor rdx, rdx
call _V17assign_bitwise_orxx_rx
ret

.section .data

