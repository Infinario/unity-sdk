//
//  Http.m
//  InfinarioSDK
//
//  Created by Igi on 2/4/15.
//  Copyright (c) 2015 Infinario. All rights reserved.
//

#import "Http.h"

@interface Http ()

@property NSString *target;

@end

@implementation Http

- (instancetype)initWithTarget:(NSString *)target {
    self = [super init];
    
    self.target = target ? target : @"https://api.infinario.com";
    
    return self;
}

- (NSDictionary *)post:(NSString *)url withPayload:(NSDictionary *)payload {
    return [self httpRequest:url usingMethod:@"POST" withPayload:payload];
}

-(NSDictionary *)get:(NSString *)url {
    return [self httpRequest:url usingMethod:@"GET" withPayload:@{}];
}

- (NSDictionary *)httpRequest:(NSString *)url usingMethod:(NSString *)method withPayload:(NSDictionary *)payload {
    NSMutableURLRequest *request = [[NSMutableURLRequest alloc] initWithURL:[NSURL URLWithString:[NSString stringWithFormat:@"%@/%@", self.target, url]]];
    NSHTTPURLResponse *response = nil;
    NSError *error = nil;
    NSData *responseData = nil;
    
    [request setHTTPMethod:method];
    [request setValue:@"application/json" forHTTPHeaderField:@"Content-type"];
    [request setValue:@"application/json" forHTTPHeaderField:@"Accept"];
    
    if ([method isEqualToString:@"POST"]) {
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:payload options:0 error:nil];
        
        [request setValue:[NSString stringWithFormat:@"%d", (int) [jsonData length]] forHTTPHeaderField:@"Content-length"];
        [request setHTTPBody:jsonData];
    }
    
    responseData = [NSURLConnection sendSynchronousRequest:request returningResponse:&response error:&error];
    
    if (responseData) {
        return (NSDictionary *) [NSJSONSerialization JSONObjectWithData:responseData options:0 error:nil];
    }
    else {
        return nil;
    }
}

@end
