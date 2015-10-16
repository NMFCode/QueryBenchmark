# Create memory consumption plot

memoryConsumption = read.csv2("ergebnisseMemory.csv")
size = memoryConsumption$Size
classic = as.vector(memoryConsumption$Classic)
batch = as.vector(memoryConsumption$Batch)
inc = as.vector(memoryConsumption$Incremental)

pdf(file="..\\img\\memoryConsumption.pdf", width=6, height=3)
par(mar=c(4.3,4.0,0,0))
plot(size, inc, type="n", ylim=c(0.01,22), xlab="Number of teams (n)", ylab="Memory consumed (MiB)", log="y")
lines(size, classic, col="blue")
points(size, classic, pch=16, col="blue")
lines(size, batch, col="red")
points(size, batch, pch=2, col="red")
lines(size, inc, col="purple")
points(size, inc, pch=8, col="purple")
legend(400, 0.3, c("LINQ to Objects", "NMF Expressions (Batch)", "NMF Expressions (Incremental)"), col=c('blue', 'red', 'purple'), pch=c(16,2,8), bty='n', lty=1)
dev.off()

ergebnisse = read.csv2("ergebnisse.csv")
size = ergebnisse$Size
classic = as.vector(ergebnisse$Classic)
batch = as.vector(ergebnisse$Batch)
inc = as.vector(ergebnisse$Incremental)

pdf(file="..\\img\\ergebnisse.pdf", width=6, height=3)
par(mar=c(4.3,4.0,0,0))
plot(size, batch, type="n", xlab="Number of teams (n)", ylab="Runtime for 1000 changes [s]")
lines(size, classic, col="blue")
points(size, classic, pch=16, col="blue")
lines(size, batch, col="red")
points(size, batch, pch=2, col="red")
lines(size, inc, col="purple")
points(size, inc, pch=8, col="purple")
legend(20, 6, c("LINQ to Objects", "NMF Expressions (Batch)", "NMF Expressions (Incremental)"), col=c('blue', 'red', 'purple'), pch=c(16,2,8), bty='n', lty=1)
dev.off()