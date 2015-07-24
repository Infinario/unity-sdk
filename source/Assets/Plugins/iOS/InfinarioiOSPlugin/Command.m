//
//  Command.m
//  InfinarioSDK
//
//  Created by Igi on 2/5/15.
//  Copyright (c) 2015 Infinario. All rights reserved.
//

#import "Command.h"

@interface Command ()

@property NSString *endpoint;
@property NSNumber *timestamp;

@end

@implementation Command

- (instancetype)initWithEndpoint:(NSString *)endpoint {
    return [self initWithEndpoint:endpoint andTimestamp:nil];
}

- (instancetype)initWithEndpoint:(NSString *)endpoint andTimestamp:(NSNumber *)timestamp {
    self = [self init];
    
    self.endpoint = endpoint;
    self.timestamp = timestamp ? timestamp : [self nowInSeconds];
    
    return self;
}

- (NSNumber *)nowInSeconds {
    return [NSNumber numberWithLong:[[NSDate date] timeIntervalSince1970]];
}

- (NSMutableDictionary *)getData {
    return [[NSMutableDictionary alloc] init];
}

- (NSMutableDictionary *)getPayload {
    NSMutableDictionary *payload = [[NSMutableDictionary alloc] init];
    
    payload[@"name"] = self.endpoint;
    payload[@"data"] = [self getData];
    
    return payload;
}

@end
