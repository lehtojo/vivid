.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V17internal_allocatex_rPh

.global _VN5Apple4initEv_rPS_
_VN5Apple4initEv_rPS_:
sub rsp, 40
mov rcx, 24
call _V8allocatex_rPh
movsd xmm0, qword ptr [rip+_VN5Apple4initEv_rPS__C0]
movsd qword ptr [rax+16], xmm0
mov qword ptr [rax+8], 100
add rsp, 40
ret

.global _VN3Car4initEd_rPS_
_VN3Car4initEd_rPS_:
push rbx
sub rsp, 32
mov rcx, 32
movsd qword ptr [rsp+48], xmm0
call _V8allocatex_rPh
lea rcx, [rip+_VN3Car4initEd_rPS__S0]
mov rbx, rax
call _VN6String4initEPh_rPS_
mov qword ptr [rbx+24], rax
mov qword ptr [rbx+16], 2000000
movsd xmm0, qword ptr [rsp+48]
movsd qword ptr [rbx+8], xmm0
mov rax, rbx
add rsp, 32
pop rbx
ret

.global _V12create_applev_rP5Apple
_V12create_applev_rP5Apple:
sub rsp, 40
call _VN5Apple4initEv_rPS_
add rsp, 40
ret

.global _V10create_card_rP3Car
_V10create_card_rP3Car:
sub rsp, 40
call _VN3Car4initEd_rPS_
add rsp, 40
ret

.global _V4initv_rx
_V4initv_rx:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
call _V12create_applev_rP5Apple
pxor xmm0, xmm0
call _V10create_card_rP3Car
ret

.section .data

_VN5Apple_configuration:
.quad _VN5Apple_descriptor

_VN5Apple_descriptor:
.quad _VN5Apple_descriptor_0
.long 24
.long 0

_VN5Apple_descriptor_0:
.ascii "Apple"
.byte 0
.byte 1
.byte 2
.byte 0

_VN3Car_configuration:
.quad _VN3Car_descriptor

_VN3Car_descriptor:
.quad _VN3Car_descriptor_0
.long 32
.long 0

_VN3Car_descriptor_0:
.ascii "Car"
.byte 0
.byte 1
.byte 2
.byte 0

.balign 16
_VN3Car4initEd_rPS__S0:
.ascii "Flash"
.byte 0

.balign 16
_VN5Apple4initEv_rPS__C0:
.byte 154, 153, 153, 153, 153, 153, 185, 63 # 0.1

