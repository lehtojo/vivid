section .text
global _start
_start:
call _V4initv_rc
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh

global _V10get_animalv_rP6Animal
_V10get_animalv_rP6Animal:
sub rsp, 8
call _VN6Animal4initEv_rPh
add rsp, 8
ret

global _V8get_fishv_rP4Fish
_V8get_fishv_rP4Fish:
sub rsp, 8
call _VN4Fish4initEv_rPh
add rsp, 8
ret

global _V10get_salmonv_rP6Salmon
_V10get_salmonv_rP6Salmon:
sub rsp, 8
call _VN6Salmon4initEv_rPh
add rsp, 8
ret

global _V12animal_movesP6Animal
_V12animal_movesP6Animal:
sub rsp, 8
call _VN6Animal4moveEv
add rsp, 8
ret

global _V10fish_movesP4Fish
_V10fish_movesP4Fish:
push rbx
sub rsp, 16
lea rcx, [rdi-3]
movsx rax, byte [rcx+9]
test rax, rax
jne _V10fish_movesP4Fish_L0
lea rsi, [rdi-3]
mov rbx, rdi
call _VN4Fish4swimEP6Animal
mov rdi, rbx
_V10fish_movesP4Fish_L0:
add rsp, 16
pop rbx
ret

global _V10fish_swimsP6Animal
_V10fish_swimsP6Animal:
sub rsp, 8
mov rsi, rdi
mov rax, rdi
lea rdi, [rax+3]
call _VN4Fish4swimEP6Animal
add rsp, 8
ret

global _V10fish_stopsP6Animal
_V10fish_stopsP6Animal:
sub rsp, 8
mov rax, rdi
lea rdi, [rax+3]
call _VN4Fish5floatEv
add rsp, 8
ret

global _V10fish_hidesP6Salmon
_V10fish_hidesP6Salmon:
push rbx
sub rsp, 16
mov rax, rdi
lea rdi, [rax+3]
mov rbx, rax
call _V10fish_movesP4Fish
mov rdi, rbx
call _VN6Salmon4hideEv
add rsp, 16
pop rbx
ret

global _V17fish_stops_hidingP6Salmon
_V17fish_stops_hidingP6Salmon:
push rbx
sub rsp, 16
mov rbx, rdi
call _VN6Salmon11stop_hidingEv
lea rdi, [rbx+3]
mov rsi, rbx
call _VN4Fish4swimEP6Animal
add rsp, 16
pop rbx
ret

_V4initv_rc:
push rbx
push rbp
sub rsp, 8
mov rax, 1
add rsp, 8
pop rbp
pop rbx
ret
mov rdi, 10
call _VN11Salmon_Gang4initEx_rPh
call _V10get_animalv_rP6Animal
mov rbx, rax
call _V8get_fishv_rP4Fish
mov rbp, rax
call _V10get_salmonv_rP6Salmon
mov rdi, rbx
mov rbx, rax
call _V12animal_movesP6Animal
mov rdi, rbp
call _V10fish_movesP4Fish
mov rdi, rbx
call _V10fish_swimsP6Animal
mov rdi, rbx
call _V10fish_stopsP6Animal
mov rdi, rbx
call _V10fish_hidesP6Salmon
mov rdi, rbx
call _V17fish_stops_hidingP6Salmon
pop rbp
pop rbx
ret

_VN6Animal4initEv_rPh:
sub rsp, 8
mov rdi, 3
call _V8allocatex_rPh
mov word [rax], 100
mov byte [rax+2], 0
add rsp, 8
ret

_VN6Animal4moveEv:
sub word [rdi], 1
add byte [rdi+2], 1
ret

_VN4Fish4initEv_rPh:
sub rsp, 8
mov rdi, 6
call _V8allocatex_rPh
mov word [rax], 1
mov word [rax+2], 0
mov word [rax+4], 1500
add rsp, 8
ret

_VN4Fish4swimEP6Animal:
push rbx
sub rsp, 16
mov rax, rdi
mov rdi, rsi
mov rbx, rax
call _VN6Animal4moveEv
movsx rax, word [rbx]
mov word [rbx+2], ax
add rsp, 16
pop rbx
ret

_VN4Fish5floatEv:
mov word [rdi+2], 0
ret

_VN6Salmon4initEv_rPh:
sub rsp, 8
mov rdi, 10
call _V8allocatex_rPh
mov byte [rax+9], 0
mov word [rax+3], 5
mov word [rax+7], 5000
add rsp, 8
ret

_VN6Salmon4hideEv:
push rbx
sub rsp, 16
mov rax, rdi
lea rdi, [rax+3]
mov rbx, rax
call _VN4Fish5floatEv
mov byte [rbx+9], 1
add rsp, 16
pop rbx
ret

_VN6Salmon11stop_hidingEv:
push rbx
sub rsp, 16
mov rsi, rdi
mov rax, rdi
lea rdi, [rax+3]
mov rbx, rax
call _VN4Fish4swimEP6Animal
mov byte [rbx+9], 0
add rsp, 16
pop rbx
ret

_VN11Salmon_Gang4initEx_rPh:
push rbx
sub rsp, 16
mov rcx, rdi
mov rdi, 8
mov rbx, rcx
call _V8allocatex_rPh
mov qword [rax], 1
mov qword [rax], rbx
add rsp, 16
pop rbx
ret