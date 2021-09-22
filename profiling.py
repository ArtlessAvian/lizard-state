import statistics

a = input()
times = []
while True:
    try:
        times.append(int(a))
        a = input()
    except:
        break

print(max(times))
print(min(times))
print(statistics.mean(times))
print(statistics.stdev(times))