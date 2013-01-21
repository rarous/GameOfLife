module GameOfLife

open System
open System.Reactive
open System.Reactive.Linq

open System.Collections.Generic
open FsUnit.Xunit
open Microsoft.Reactive.Testing
open NSubstitute
open Xunit

type Generation() = class end

type Rules() = class end

type World(generation: Generation, rules: Rules, clock: IObservable<int64>) =
    let computeNewGeneration() = Generation()

    interface IObservable<Generation> with
        member x.Subscribe obs = clock.Select(fun _ -> computeNewGeneration()).Subscribe(obs)

let onNext ticks value =
    Recorded<Notification<int64>>(int64 ticks, Notification.CreateOnNext(int64 value))

[<Fact>]
let ``Every clock tick world generates new generation`` () =
    let scheduler = new TestScheduler()
    let clock = scheduler.CreateHotObservable(onNext 500 1, onNext 1000 2)
    let world = World(Generation(), Rules(), clock)
    let gens = List<Generation>()
    world.Subscribe(gens.Add) |> ignore
    scheduler.AdvanceBy(int64 1001)
    gens.Count |> should equal 2
