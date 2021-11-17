ret

push $register64
neg $register
neg $address

mov $register64, $constant64

mov $register64, $register64
mov $register32, $register32
mov $register16, $register16
mov $register8, $register8

add $address64, $register64
add $address32, $register32
add $address16, $register16
add $address8, $register8

sub $register64, $address64
sub $register32, $address32
sub $register16, $address16
sub $register8, $address8

sub $register64, $minus?$constant32
sub $register32, $minus?$constant32
sub $register16, $minus?$constant16
sub $register8, $minus?$constant8

sub qword $ptr [rsp $sign $constant-max-32], $minus?$constant32
sub dword $ptr [rsp $sign $constant-max-32], $minus?$constant32
sub word $ptr [rsp $sign $constant-max-32], $minus?$constant16
sub byte $ptr [rsp $sign $constant-max-32], $minus?$constant8

sub qword $ptr [rsp $sign $constant-max-32], $register64
sub dword $ptr [rsp $sign $constant-max-32], $register32
sub word $ptr [rsp $sign $constant-max-32], $register16
sub byte $ptr [rsp $sign $constant-max-32], $register8

sub $register64, qword $ptr [rsp $sign $constant-max-32]
sub $register32, dword $ptr [rsp $sign $constant-max-32]
sub $register16, word $ptr [rsp $sign $constant-max-32]
sub $register8, byte $ptr [rsp $sign $constant-max-32]

imul $register64, $constant-max-32
imul $register32, $constant-max-32
imul $register16, $constant16
imul $register16, $constant8

imul $register64, $register64, $constant-max-32
imul $register32, $register32, $constant-max-32
imul $register16, $register16, $constant16
imul $register16, $register16, $constant8

imul $register64, $address64, $constant-max-32
imul $register32, $address32, $constant-max-32
imul $register16, $address16, $constant16
imul $register16, $address16, $constant8

add rax, $constant-max-32
add eax, $constant-max-32
add ax, $constant16
add ax, $constant8
add al, $constant8

xchg rax, $register64
xchg eax, $register32
xchg ax, $register16