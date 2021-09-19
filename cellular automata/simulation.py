import numpy as np
import pygame
import sys
import copy
from pygame.locals import *
from random import random

# importing grids
classes = np.genfromtxt("Data\content\data\classes.csv", delimiter=",")
elevation = np.genfromtxt("Data/content/data/elevation.csv", delimiter=",")
moisture = np.genfromtxt("Data/content/data/moisture.csv", delimiter=",")
wind_u = np.genfromtxt("Data/content/data/wind_u.csv", delimiter=",")
wind_v = np.genfromtxt("Data/content/data/wind_v.csv", delimiter=",")


state = {
    0: 0,
    1: 0,
    2: 1,
    3: 1
}

# cell states :

# 0: burnable (vegetation)
# 1: not burnable (water or soil)
# 2: ignited
# 3: on fire
# 4: extenguishing
# 5: fully extenguished


class cell:
    def __init__(self, land_type, elevation, moisture, wind_u, wind_v):
        self.type = land_type
        self.elevation = elevation
        self.moisture = moisture
        self.wind_u = wind_u
        self.wind_v = wind_v
        self.veg_hight = 0
        self.state = state[land_type]
        self.prev_state = self.state


    def set_state(self, new_state):
        self.prev_state = self.state
        self.state = new_state


    def update_cell(self):
        if self.state == 2 and self.prev_state == 2:
            self.set_state(3)
            return
        elif self.state == 3 and self.prev_state == 3:
            self.set_state(4)
            return
        elif self.state == 4 and self.state == 4:
            self.set_state(5)
            return
        self.prev_state = self.state


thresh_up = 1.1
thresh_down = 0.99
thresh_wind = -0.99
R_0 = 1


class CA:
    def __init__(self, classes, elevation_data, moisture_data, wind_u, wind_v, shape):
        self.grid = []
        for j in range(shape[0]):
            row = []
            for i in range(shape[1]):
                row.append(cell(classes[j, i], elevation_data[j, i],
                           moisture_data[j, i], wind_u[j, i], wind_v[j, i]))
            self.grid.append(row)
        self.grid = np.array(self.grid)
        self.new_grid = copy.deepcopy(self.grid)

    def nighbours(self, j, i):
        return np.array([self.new_grid[j-1, i-1],
                         self.new_grid[j-1, i],
                         self.new_grid[j-1, i+1],
                         self.new_grid[j, i+1],
                         self.new_grid[j+1, i+1],
                         self.new_grid[j+1, i],
                         self.new_grid[j+1, i-1],
                         self.new_grid[j, i-1]])

    def direction(self):
        return np.array([[1/np.sqrt(2), -1/np.sqrt(2)], [0, 1], [1/np.sqrt(2), 1/np.sqrt(2)], [1, 0], [1/np.sqrt(2), -1/np.sqrt(2)], [-1, 0], [-1/np.sqrt(2), -1/np.sqrt(2)], [0, -1]])

    def set_fire(self, j, i):
        self.grid[j, i].set_state(3)

    def slope(self, source, cell):
        return np.arctan((cell.elevation-source.elevation)/30)*180/np.pi

    def slope_factor(self, slope):
        if slope < 0:
            return np.exp(-0.069*slope)/(2*np.exp(-0.069*slope)-1)
        else:
            return np.exp(0.069*slope)

    def ratio(self, source, cell, dir):
        return R_0*self.slope_factor(self.slope(source, cell))
        #np.dot([source.wind_u/np.sqrt(source.wind_u**2+source.wind_v**2), source.wind_v/np.sqrt(source.wind_u**2+source.wind_v**2)], dir)
        # *(1+0.2*np.sqrt(source.wind_u**2+source.wind_v**2))

    def update(self):
        for j in range(1, self.grid.shape[0]-1):
            for i in range(1, self.grid.shape[1]-1):
                self.grid[j, i].set_state(self.new_grid[j, i].state)

    def simulate(self):
        for j in range(1, self.grid.shape[0]-1):
            for i in range(1, self.grid.shape[1]-1):
                self.new_grid[j, i].update_cell()
                if self.grid[j, i].state == 3:
                    for n, d in list(filter(lambda x: x[0].state == 0, list(zip(self.nighbours(j, i), self.direction())))):
                        if self.ratio(self.grid[j, i], n, d) > thresh_up:
                            n.set_state(3)
                        elif self.ratio(self.grid[j, i], n, d) > thresh_down:
                            n.set_state(2)
                        # elif self.ratio(self.grid[j, i], n, d) < thresh_wind:
                            # n.set_state(2)

        self.update()


ca = CA(classes, elevation, moisture, wind_u, wind_v, (241, 241))
ca.set_fire(70, 30)
#ca.set_fire(200, 50)
bach = 30

# Colours
BACKGROUND = (255, 255, 255)

# Game Setup
FPS = 60
fpsClock = pygame.time.Clock()
WINDOW_HEIGHT, WINDOW_WIDTH = classes.shape
WINDOW_HEIGHT, WINDOW_WIDTH = WINDOW_HEIGHT*2, WINDOW_WIDTH*2

# 0: burnable (vegetation)
# 1: not burnable (water or soil)
# 2: ignited
# 3: on fire
# 4: extenguishing
# 5: fully extenguished
color = {
    0: "#4f772d",
    1: "#0096c7",
    2: "#dc2f02",
    3: "#d00000",
    4: "#ffba08",
    5: "#161a1d"
}
WINDOW = None
WINDOW = pygame.display.set_mode((WINDOW_WIDTH, WINDOW_HEIGHT), DOUBLEBUF)
WINDOW.set_alpha(None)
pygame.display.set_caption('simulation')

# The main function that controls the game


def ci(x, y, c):
    rectangle1 = pygame.Rect(x, y, 2, 2)
    pygame.draw.rect(WINDOW, color[c], rectangle1)


def main():
    looping = True
    # The main game loop
    while looping:
        # Get inputs
        for event in pygame.event.get():
            if event.type == QUIT:
                pygame.quit()
                sys.exit()

        # Processing
        # This section will be built out later
        # Render elements of the game
        # ca.update()
        WINDOW.fill(BACKGROUND)

        for i in range(bach):
            ca.simulate()

        f = ca.grid
        for j in range(0, 241):
            for i in range(0, 241):
                ci(i*2, j*2, f[j, i].state)

        pygame.display.update()
        fpsClock.tick(30)


main()
pygame.quit()
