.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V17internal_allocatex_rPh

.global _VN6Holder4initEv_rPS_
_VN6Holder4initEv_rPS_:
sub rsp, 40
mov rcx, 31
call _V8allocatex_rPh
add rsp, 40
ret

.global _V10assignmentP6Holder
_V10assignmentP6Holder:
mov dword ptr [rcx+8], 314159265
mov byte ptr [rcx+12], 64
movsd xmm0, qword ptr [rip+_V10assignmentP6Holder_C0]
movsd qword ptr [rcx+13], xmm0
mov rdx, -2718281828459045
mov qword ptr [rcx+21], rdx
mov word ptr [rcx+29], 12345
ret

.global _V4initv_rx
_V4initv_rx:
mov rax, 1
ret

.section .data

_VN6Holder_configuration:
.quad _VN6Holder_descriptor

_VN6Holder_descriptor:
.quad _VN6Holder_descriptor_0
.long 31
.long 0

_VN6Holder_descriptor_0:
.ascii "Holder"
.byte 0
.byte 1
.byte 2
.byte 0

.balign 16
_V10assignmentP6Holder_C0:
.byte 57, 180, 200, 118, 190, 159, 246, 63 # 1.414

