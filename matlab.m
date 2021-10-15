arrayPhase = ones(11,11);

% Brazo derecha
% arrayPhase(7,6) = 1;
% arrayPhase(8,6) = 1;
% 
% % Brazo arriba
% arrayPhase(6,5) = 1;
% arrayPhase(6,4) = 1;
% 
% % Brazo abajo
% arrayPhase(6,7) = 1;
% arrayPhase(6,8) = 1;
% 
% % Brazo izquierda
% arrayPhase(4,6) = 1;
% arrayPhase(5,6) = 1;

arrayTemp = -1*ones(11,11);

% Brazo derecha
% arrayTemp(7,6) = -1;
% arrayTemp(8,6) = -1;
% 
% % Brazo arriba
% arrayTemp(6,5) = -1;
% arrayTemp(6,4) = -1;
% 
% % Brazo abajo
% arrayTemp(6,7) = -1;
% arrayTemp(6,8) = -1;
% 
% % Brazo izquierda
% arrayTemp(4,6) = -1;
% arrayTemp(5,6) = -1;

array2phase = ones(11,11);
array2temp = -1*ones(11,11);

arrayPhase(6,6) = 0;
arrayTemp(6,6) = 0;

m = 20;
dt = 5e-6;
d = 0.5;
e = 0.005;
b = 400;
dx = 0.005;
dy = 0.005;

% Rules(20, 20, 5e-6, 0.5, 0.005, 400, 0.005, 0.005);
for z = 1:1
    for i=2:10
        for j=2:10
            dPHI2dxy = (arrayPhase(i+1,j) - 2 * arrayPhase(i,j) + arrayPhase(i-1,j)) / dx / dx + (arrayPhase(i,j+1) - 2 * arrayPhase(i,j) + arrayPhase(i,j-1)) / dy / dy;
            dPHIdt = 1 / e / e / m * (arrayPhase(i,j) * (1 - arrayPhase(i,j)) * (arrayPhase(i,j) - 1 / 2 + 30 * e * b * d * arrayTemp(i,j) * arrayPhase(i,j) * (1 - arrayPhase(i,j))) + e * e * dPHI2dxy);
            if (i==6 && j==6)
               asd = dPHIdt; 
            end
            du2dxy = (arrayTemp(i+1,j) - 2 * arrayTemp(i,j) + arrayTemp(i-1,j)) / dx / dx + (arrayTemp(i,j+1) - 2 * arrayTemp(i,j) + arrayTemp(i,j-1)) / dy / dy;
            dudt = du2dxy - 1 / d * (30 * arrayPhase(i,j)^ 2 - 60 * arrayPhase(i,j)^ 3 + 30 * arrayPhase(i,j)^4) * dPHIdt;
            array2phase(i,j) = arrayPhase(i,j) + dPHIdt * dt;
            array2temp(i,j) = arrayTemp(i,j) + dudt * dt;
        end
    end
    arrayTemp = array2temp;
    arrayPhase = array2phase;
end