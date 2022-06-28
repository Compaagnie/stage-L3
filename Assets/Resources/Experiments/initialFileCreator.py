group = 2
groupMove = [["drag","TP","joy"],["TP","joy","drag"]]
cardList = [12,28,40,2,20,15]
lenght = (int) (len(cardList)/3)

file = open("initialTrialFile.txt", "w")
file.write("Group;Participant;CollabEnvironememn;MoveMode;CardToTag;\n");

for g in range(1, group+1):
    for n in range(0, lenght):
        if n%2 == 0:
            file.write("g0" + str(g) + ";p01;C;" + groupMove[g-1][0] + ";" + str(cardList[n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + "sync" + ";" + str(cardList[n]) + "\n")
        else:
            file.write("g0" + str(g) + ";p01;C;" + "sync" + ";" + str(cardList[n]) + "\n")
            file.write("g0" + str(g) + ";p02;C;" + groupMove[g-1][0] + ";" + str(cardList[n]) + ";\n")
    file.write("#pause;\n")
    for n in range(lenght, 2*lenght):
        if n%2 == 0:
            file.write("g0" + str(g) + ";p01;C;" + groupMove[g-1][1] + ";" + str(cardList[n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + "sync" + ";" + str(cardList[n]) + "\n")
        else:
            file.write("g0" + str(g) + ";p01;C;" + "sync" + ";" + str(cardList[n]) + "\n")
            file.write("g0" + str(g) + ";p02;C;" + groupMove[g-1][1] + ";" + str(cardList[n]) + ";\n")
    file.write("#pause;\n")
    for n in range(2*lenght, 3*lenght):
        if n%2 == 0:
            file.write("g0" + str(g) + ";p01;C;" + groupMove[g-1][2] + ";" + str(cardList[n]) + ";\n")
            file.write("g0" + str(g) + ";p02;C;" + "sync" + ";" + str(cardList[n]) + "\n")
        else:
            file.write("g0" + str(g) + ";p01;C;" + "sync" + ";" + str(cardList[n]) + "\n")
            file.write("g0" + str(g) + ";p02;C;" + groupMove[g-1][2] + ";" + str(cardList[n]) + ";\n")
    if g != group:
        file.write("#pause;\n")
    else:
        file.write("#pause;")

file.close()
